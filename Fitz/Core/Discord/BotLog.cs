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
                LogConsoleSettings.Commands => await this.dClient.GetChannelAsync(Waterbear.loggingChannel),
                LogConsoleSettings.Jobs => await this.dClient.GetChannelAsync(Waterbear.Jobs),
                LogConsoleSettings.LotteryLog => await this.dClient.GetChannelAsync(Waterbear.LotteryLog),
            };
            try
            {
                await channel.SendMessageAsync($"**[{DateTime.UtcNow}]** {DiscordEmoji.FromGuildEmote(this.dClient, emoji)} {message}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error sending message to log channel\nMessage: {message}");
            }
        }

        public async void Error(string message)
        {
            Log.Error(message);
            DiscordChannel logChannel = await this.dClient.GetChannelAsync(Waterbear.Exceptions);
            await logChannel.SendMessageAsync(message);
        }
    }
}