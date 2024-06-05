using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.ModalCommands;
using DSharpPlus.ModalCommands.Extensions;
using DSharpPlus.SlashCommands;
using Fitz.Core.Commands;
using Fitz.Core.Commands.Attributes;
using Fitz.Core.Commands.Settings;
using Fitz.Core.Services.Features;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Commands;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Features.Blackjack.Commands;
using Fitz.Features.Polls.Polls;
using Fitz.Variables;
using Fitz.Variables.Channels;
using Fitz.Variables.Emojis;
using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Fitz.Core.Discord
{
    public class Bot(IServiceProvider provider, IServiceScopeFactory scopeFactory, ActivityManager activityManager, BotLog botLog, DiscordClient dClient, IAudioService audioService) : Feature
    {
        private readonly IServiceProvider provider = provider;
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly ActivityManager activityManager = activityManager;
        private readonly BotLog botLog = botLog;
        private readonly DiscordClient dClient = dClient;
        private readonly IAudioService audioService = audioService;
        private CommandsNextExtension cNext;
        private SlashCommandsExtension slash;
        private ModalCommandsExtension modals;

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

            this.cNext.RegisterCommands<PrivateCommands>();
            this.slash.RegisterCommands<GeneralSlashCommands>();
            //this.slash.RegisterCommands<PollSlashCommands>();

            //this.cNext.RegisterCommands<PublicCommands>();
            this.slash.RegisterCommands<SettingsCommands>();
            this.slash.RegisterCommands<PollSlashCommands>();
            this.slash.RegisterCommands<BlackjackSlashCommands>();
            this.modals.RegisterModals<SettingsModalComands>();
            this.modals.RegisterModals<PollModalCommands>();
            this.modals.RegisterModals<AccountModalCommands>();

            var playerOptions = new LavalinkPlayerOptions
            {
                InitialTrack = new TrackQueueItem("https://www.youtube.com/watch?v=dQw4w9WgXcQ"),
            };

            await this.dClient.InitializeAsync();
            VariableManager.ApplyVariableScopes(this.dClient);
            await this.dClient.ConnectAsync();
        }

        #region Modal Events

        private async Task ModalCommandErrored(ModalCommandsExtension sender, DSharpPlus.ModalCommands.EventArgs.ModalCommandErrorEventArgs args)
        {
            if (args.Exception.Message != null)
            {
                Log.Error($"MODAL COMMAND ERROR: {args.Exception.Message} | Stack Trace: {args.Exception.StackTrace}");
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
        }

        #endregion Modal Events

        #region Guild Events

        private Task OnGuildAvailable(DiscordClient dClient, GuildCreateEventArgs args)
        {
            if (args.Guild.Id != DodeDuke.BotMods)
            {
                return Task.CompletedTask;
            }

            return args.Guild.RequestMembersAsync();
        }

        #endregion Guild Events

        private Task OnReady(DiscordClient dClient, SessionReadyEventArgs args)
        {
            Ready = true;
            return this.activityManager.ResetActivityAsync();
        }

        #region Socket Events

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

        #endregion Socket Events

        #region Command Events

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

        #endregion Command Events

        #region Slash Command Events

        private async Task OnSlashCommandErrored(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.SlashCommandErrorEventArgs args)
        {
            if (args.Exception is SlashExecutionChecksFailedException ex)
            {
                foreach (SlashCheckBaseAttribute check in ex.FailedChecks)
                {
                    #region RequireAccount

                    if (check is RequireAccount)
                    {
                        await args.Context.DeferAsync(true);
                        DiscordButtonComponent accpetBtn = new(DiscordButtonStyle.Success, "signup_confirm", "Confirm", false);
                        DiscordButtonComponent cancelBtn = new(DiscordButtonStyle.Danger, "singup_cancel", "Cancel", false);

                        await args.Context.FollowUpAsync(
                            new DiscordFollowupMessageBuilder()
                            .WithContent($"It doesn't seem like you have an account. Would you like to create one?")
                            .AddComponents(cancelBtn, accpetBtn)
                            .AsEphemeral(true));

                        args.Context.Client.ComponentInteractionCreated += async (s, e) =>
                        {
                            // If the confirm button was pressed
                            if (e.Id == "signup_confirm")
                            {
                                try
                                {
                                    DiscordGuild guild = await args.Context.Client.GetGuildAsync(Guilds.Waterbear);
                                    DiscordMember discordMember = await guild.GetMemberAsync(args.Context.User.Id);
                                    if (discordMember == null)
                                    {
                                        await args.Context.EditFollowupAsync(e.Message.Id, new DiscordWebhookBuilder().WithContent("You need to be in the Waterbear guild to create an account."));
                                        return;
                                    }
                                    await discordMember.GrantRoleAsync(guild.GetRole(Roles.Accounts));

                                    // Create an account for the user.
                                    AccountService accountService = args.Context.Services.GetService<AccountService>();
                                    var accountCreationResult = await accountService.CreateAccountAsync(args.Context.User);

                                    if (accountCreationResult.Success == true)
                                    {
                                        // Give user account creation beer
                                        BankService bankService = args.Context.Services.GetService<BankService>();
                                        await bankService.AwardAccountCreationBonusAsync(accountCreationResult.Data as Account);

                                        // Get account details from db.
                                        Account account = accountService.FindAccount(args.Context.User.Id);
                                        DiscordEmbedBuilder accountEmbed = new DiscordEmbedBuilder
                                        {
                                            Footer = new DiscordEmbedBuilder.EmbedFooter
                                            {
                                                IconUrl = DiscordEmoji.FromGuildEmote(args.Context.Client, ManageRoleEmojis.Warning).Url,
                                                Text = $"Account Creation",
                                            },
                                            Color = new DiscordColor(52, 114, 53),
                                            Timestamp = DateTime.UtcNow,
                                            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                                            {
                                                Url = args.Context.User.AvatarUrl,
                                            },
                                            Description = "I collect beer and stupid user data."
                                        };
                                        accountEmbed.AddField($"**Beer**", $"{account.Beer}", true);
                                        accountEmbed.AddField($"**Lifetime Beer**", $"{account.LifetimeBeer}", true);
                                        accountEmbed.AddField($"**Favorability**", $"{account.Favorability}", true);
                                        await args.Context.EditFollowupAsync(e.Message.Id, new DiscordWebhookBuilder().AddEmbed(accountEmbed.Build()).WithContent($"Account created. Please try running your original command again."));
                                    }
                                    return;
                                }
                                catch (Exception ex)
                                {
                                    return;
                                }
                            }
                            // if the cancel button was pressed
                            else if (e.Id == "singup_cancel")
                            {
                                await args.Context.EditFollowupAsync(e.Message.Id, new DiscordWebhookBuilder().WithContent("You will need an account to run that command. Use `/signup` to get started."));
                                return;
                            }
                        };
                        return;
                    }

                    #endregion RequireAccount
                }
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

            return;
        }

        #endregion Slash Command Events

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