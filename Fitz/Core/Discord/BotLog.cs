using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Variables.Channels;
using Serilog;
using System;

namespace Fitz.Core.Discord
{
    public sealed class BotLog
    {
        private readonly DiscordClient dClient;

        public BotLog(DiscordClient dClient)
        {
            this.dClient = dClient;
        }

        public async void Information(LogConsoleSettings consoleChannel, ulong emoji, string message)
        {
            Log.Information(message);
            DiscordChannel channel = consoleChannel switch
            {
                LogConsoleSettings.None => throw new NotImplementedException(),
                LogConsoleSettings.Commands => await this.dClient.GetChannelAsync(DodeDuke.Commands),
                LogConsoleSettings.Jobs => await this.dClient.GetChannelAsync(DodeDuke.Jobs),
                LogConsoleSettings.LotteryLog => await this.dClient.GetChannelAsync(DodeDuke.LotteryLog),
                LogConsoleSettings.Console => throw new NotImplementedException(),
                LogConsoleSettings.RoleEdits => throw new NotImplementedException(),
                LogConsoleSettings.UserInfo => throw new NotImplementedException(),
                LogConsoleSettings.AccountLog => await this.dClient.GetChannelAsync(DodeDuke.AccountLog),
                LogConsoleSettings.Transactions => await this.dClient.GetChannelAsync(DodeDuke.Transactions),
                LogConsoleSettings.PollLog => await this.dClient.GetChannelAsync(DodeDuke.PollLog),
                LogConsoleSettings.RenameLog => await this.dClient.GetChannelAsync(DodeDuke.RenameLog),
            };
            try
            {
                await channel.SendMessageAsync($"**[{DateTime.UtcNow}]** {DiscordEmoji.FromGuildEmote(this.dClient, emoji)} {message}");
                Console.WriteLine($"**[{DateTime.UtcNow}]** {message}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error sending message to log channel\nMessage: {message}");
            }
        }

        public async void Error(string message)
        {
            Log.Error(message);
            Console.WriteLine($"**[{DateTime.UtcNow}]** {message}");
            DiscordChannel logChannel = await this.dClient.GetChannelAsync(DodeDuke.Exceptions);
            await logChannel.SendMessageAsync(message);
        }

        internal void Information(LogConsoleSettings accountLog, string v)
        {
            throw new NotImplementedException();
        }
    }
}