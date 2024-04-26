using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.ModalCommands;
using DSharpPlus.ModalCommands.Extensions;
using DSharpPlus.SlashCommands;
using Fitz.Core.Commands;
using Fitz.Core.Services.Features;
using Fitz.Features.Accounts;
using Fitz.Features.Polls.Polls;
using Fitz.Variables;
using Fitz.Variables.Channels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Fitz.Core.Discord
{
    public class Bot : Feature
    {
        private readonly IServiceProvider provider;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ActivityManager activityManager;
        private readonly BotLog botLog;
        private readonly DiscordClient dClient;
        private CommandsNextExtension cNext;
        private SlashCommandsExtension slash;
        private ModalCommandsExtension modals;

        public Bot(IServiceProvider provider, IServiceScopeFactory scopeFactory, ActivityManager activityManager, BotLog botLog, DiscordClient dClient)
        {
            this.provider = provider;
            this.scopeFactory = scopeFactory;
            this.activityManager = activityManager;
            this.botLog = botLog;
            this.dClient = dClient;
        }

        public static bool Ready { get; private set; }

        public static WebSocketState SocketState { get; private set; }

        public override string Name => "Fitz";

        public override string Description => "You cannot disable the bot. Just shut it down. Like, what are you doing?";

        public override bool Protected => true;

        public override Task Initialize()
        {
            this.cNext = this.dClient.UseCommandsNext(new CommandsNextConfiguration
            {
                Services = this.provider,
                StringPrefixes = Environment.GetEnvironmentVariable("COMMAND_PREFIXES").Split(","),
            });

            this.slash = this.dClient.UseSlashCommands(new SlashCommandsConfiguration
            {
                Services = this.provider,
            });

            this.modals = this.dClient.UseModalCommands(new ModalCommandsConfiguration
            {
                Services = this.provider,
            });

            AppDomain.CurrentDomain.ProcessExit += this.OnShutdown;

            return base.Initialize();
        }

        public override async Task Enable()
        {
            this.dClient.GuildAvailable += this.OnGuildAvailable;
            this.dClient.SocketOpened += this.OnSocketOpened;
            this.dClient.SocketClosed += this.OnSocketClosed;
            this.dClient.SocketErrored += this.OnSocketErrored;
            this.dClient.SessionCreated += this.OnReady;

            this.cNext.CommandErrored += this.OnCommandErroredAsync;
            this.cNext.CommandExecuted += this.OnCommandExecuted;

            this.slash.SlashCommandExecuted += this.OnSlashCommandExecuted;
            this.slash.SlashCommandErrored += this.OnSlashCommandErrored;
            this.slash.ContextMenuErrored += this.OnContextMenuErrored;

            this.modals.ModalCommandExecuted += this.ModalCommandExecuted;
            this.modals.ModalCommandErrored += this.ModalCommandErrored;

            //this.cNext.RegisterCommands<PrivateCommands>();
            this.slash.RegisterCommands<GeneralSlashCommands>();
            //this.slash.RegisterCommands<PollSlashCommands>();

            //this.cNext.RegisterCommands<PublicCommands>();
            this.slash.RegisterCommands<PollSlashCommands>();
            this.modals.RegisterModals<PollModalCommands>();

            await this.dClient.InitializeAsync();
            VariableManager.ApplyVariableScopes(this.dClient);
            await this.dClient.ConnectAsync();
        }

        private async Task ModalCommandErrored(ModalCommandsExtension sender, DSharpPlus.ModalCommands.EventArgs.ModalCommandErrorEventArgs args)
        {
            if (args.Exception.Message != null)
            {
                await args.Context.Channel.SendMessageAsync("You do not have permission to run this command.");
                return;
            }

            Log.Error(args.Exception, $"Command '{args.Context.Interaction.Type}' errored");
            this.botLog.Error($"`{args.Context.User.Username}` ran `{args.Context.Interaction.Type}` in **[{args.Context.Guild?.Name ?? "DM"} - {args.Context.Channel.Name}]**: {args.Exception.Message}");
        }

        private async Task ModalCommandExecuted(ModalCommandsExtension sender, DSharpPlus.ModalCommands.EventArgs.ModalCommandExecutionEventArgs args)
        {
            string logMessage = $"`{args.Context.User.Username}` ran `/{args.Context.Interaction.Type}` in **[{(args.Context.Guild != null ? $"{args.Context.Guild.Name} - {args.Context.Channel.Name}" : "DM")}]**";

            this.botLog.Information(LogConsoleSettings.Commands, Emoji.Run, logMessage);

            Log.Debug(logMessage);

            using IServiceScope scope = this.scopeFactory.CreateScope();
        }

        private Task OnGuildAvailable(DiscordClient dClient, GuildCreateEventArgs args)
        {
            if (args.Guild.Id != DodeDuke.BotMods)
            {
                return Task.CompletedTask;
            }

            return args.Guild.RequestMembersAsync();
        }

        private Task OnReady(DiscordClient dClient, SessionReadyEventArgs args)
        {
            Ready = true;
            return this.activityManager.ResetActivityAsync();
        }

        private Task OnSocketClosed(DiscordClient dClient, SocketCloseEventArgs args)
        {
            SocketState = WebSocketState.Closed;
            return Task.CompletedTask;
        }

        private Task OnSocketErrored(DiscordClient dClient, SocketErrorEventArgs args)
        {
            SocketState = WebSocketState.Closed;
            Log.Error(args.Exception, "Socket errored");
            return Task.CompletedTask;
        }

        private Task OnSocketOpened(DiscordClient dClient, SocketEventArgs args)
        {
            SocketState = WebSocketState.Open;
            return Task.CompletedTask;
        }

        private async Task OnCommandErroredAsync(CommandsNextExtension cNext, CommandErrorEventArgs args)
        {
            if (args.Exception is ChecksFailedException)
            {
                await args.Context.Message.CreateReactionAsync(DiscordEmoji.FromName(this.dClient, ":underage:"));
                return;
            }
            else if (args.Exception is CommandNotFoundException)
            {
                if (!(args.Context.Message.Content.Length > 1 && args.Context.Message.Content[0] == args.Context.Message.Content[1]))
                {
                    await args.Context.RespondAsync($"'{args.Context.Message.Content.Split(' ')[0]}' is not a known command. See '.help'");
                }

                return;
            }

            await args.Context.Message.CreateReactionAsync(DiscordEmoji.FromName(this.dClient, ":interrobang:"));
            Log.Error(args.Exception, $"Command '{args.Context.Message.Content}' errored");
            this.botLog.Error($"`{args.Context.User.Username}` ran `{args.Context.Message.Content}` in **[{args.Context.Guild?.Name ?? "DM"} - {args.Context.Channel.Name}]**: {args.Exception.Message}");
        }

        private async Task OnCommandExecuted(CommandsNextExtension cNext, CommandExecutionEventArgs args)
        {
            string logMessage = $"`{args.Context.User.Username}` ran `{args.Context.Message.Content}` in **[{(args.Context.Guild != null ? $"{args.Context.Guild.Name} - {args.Context.Channel.Name}" : "DM")}]**";
            Log.Debug(logMessage);
            this.botLog.Information(LogConsoleSettings.Commands, Emoji.Run, logMessage);

            using IServiceScope scope = this.scopeFactory.CreateScope();

            return;
        }

        private async Task OnSlashCommandErrored(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.SlashCommandErrorEventArgs args)
        {
            if (args.Exception is SlashExecutionChecksFailedException)
            {
                await args.Context.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You do not have permission to run this command.").AsEphemeral(true));
                return;
            }

            Log.Error(args.Exception, $"Command '{args.Context.CommandName}' errored");
            this.botLog.Error($"`{args.Context.User.Username}` ran `{args.Context.CommandName}` in **[{args.Context.Guild?.Name ?? "DM"} - {args.Context.Channel.Name}]**: {args.Exception.Message}");
        }

        private async Task OnSlashCommandExecuted(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.SlashCommandExecutedEventArgs args)
        {
            string logMessage = $"`{args.Context.User.Username}` ran `/{args.Context.CommandName}` in **[{(args.Context.Guild != null ? $"{args.Context.Guild.Name} - {args.Context.Channel.Name}" : "DM")}]**";

            this.botLog.Information(LogConsoleSettings.Commands, Emoji.Run, logMessage);

            Log.Debug(logMessage);

            using IServiceScope scope = this.scopeFactory.CreateScope();

            return;
        }

        private async Task OnContextMenuErrored(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.ContextMenuErrorEventArgs args)
        {
            if (args.Exception is ContextMenuExecutionChecksFailedException)
            {
                await args.Context.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You do not have permission to do this.").AsEphemeral(true));
                return;
            }

            Log.Error(args.Exception, $"Command '{args.Context.CommandName}' errored");
            this.botLog.Error($"`{args.Context.User.Username}` ran `{args.Context.CommandName}` in **[{args.Context.Guild?.Name ?? "DM"} - {args.Context.Channel.Name}]**: {args.Exception.Message}");
        }

        private void OnShutdown(object sender, EventArgs args)
        {
            this.dClient.DisconnectAsync();
        }
    }
}