namespace Fitz.BackgroundServices
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Exceptions;
    using DSharpPlus.Entities;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;

    public class CommandHandler
    {
        private readonly IServiceProvider provider;
        private readonly FitzLog bloonLog;
        private readonly DiscordClient dClient;

        public CommandHandler(IServiceProvider provider)
        {
            this.provider = provider;
            this.bloonLog = provider.GetService<FitzLog>();
            this.dClient = provider.GetService<DiscordClient>();
        }

        public void Initialize()
        {
            CommandsNextExtension commands = this.dClient.UseCommandsNext(new CommandsNextConfiguration
            {
                Services = this.provider,
                StringPrefixes = Environment.GetEnvironmentVariable("COMMAND_PREFIXES").Split(","),
            });

            commands.CommandErrored += this.OnCommandErroredAsync;
            commands.CommandExecuted += this.OnCommandExecuted;
            commands.RegisterCommands(Assembly.GetEntryAssembly());
        }

        private async Task OnCommandErroredAsync(CommandErrorEventArgs args)
        {
            if (args.Exception is ChecksFailedException)
            {
                await args.Context.Message.CreateReactionAsync(DiscordEmoji.FromName(this.dClient, ":underage:")).ConfigureAwait(false);
                return;
            }
            else if (args.Exception is CommandNotFoundException)
            {
                if (!(args.Context.Message.Content.Length > 1 && args.Context.Message.Content[0] == args.Context.Message.Content[1]))
                {
                    await args.Context.RespondAsync($"'{args.Context.Message.Content.Split(' ')[0]}' is not a known command. See '.help'").ConfigureAwait(false);
                }

                return;
            }

            Log.Error(args.Exception, $"Command '{args.Context.Message.Content}' errored");
        }

        private Task OnCommandExecuted(CommandExecutionEventArgs args)
        {
            string logMessage = $"`{args.Context.User.Username}` ran `{args.Context.Message.Content}` in **[{args.Context.Guild?.Name ?? "DM"} - {args.Context.Channel.Name}]**";
            Log.Debug(logMessage);

            return Task.CompletedTask;
        }
    }
}
