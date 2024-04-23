using DSharpPlus;
using DSharpPlus.Entities;
using System;
using Fitz.Variables.Channels;

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
            DiscordChannel channel = consoleChannel switch
            {
                LogConsoleSettings.Commands => await this.dClient.GetChannelAsync(Channels.loggingChannel),
            };
            await channel.SendMessageAsync($"**[{DateTime.UtcNow}]** {DiscordEmoji.FromGuildEmote(this.dClient, emoji)} {message}");
        }

        public async void Error(string message)
        {
            DiscordChannel logChannel = await this.dClient.GetChannelAsync(Channels.loggingChannel);
            await logChannel.SendMessageAsync(message);
        }
    }
}