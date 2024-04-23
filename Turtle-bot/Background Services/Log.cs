namespace Fitz.BackgroundServices
{
    using System;
    using DSharpPlus;
    using DSharpPlus.Entities;
    using Fitz.Variables.Channels;
    using Fitz.Variables;

    public enum LogConsole
    {
        /// <summary>
        /// Unknown channel
        /// </summary>
        None = 0,

        /// <summary>
        /// <see cref="FitzChannels.Console"/>
        /// </summary>
        Console = 1,

        /// <summary>
        /// <see cref="FitzChannels.Commands"/>
        /// </summary>
        Commands = 2,

        /// <summary>
        /// <see cref="FitzChannels.Jobs"/>
        /// </summary>
        Jobs = 3,

        /// <summary>
        /// <see cref="FitzChannels.RoleEdits"/>
        /// </summary>
        RoleEdits = 4,

        /// <summary>
        /// <see cref="FitzChannels.SBGUserInfo"/>
        /// </summary>
        UserInfo = 5,
    }

    public class FitzLog
    {
        private readonly DiscordClient dClient;

        public FitzLog(DiscordClient dClient)
        {
            this.dClient = dClient;
        }

        public async void Information(LogConsole consoleChannel, ulong emoji, string message)
        {
            DiscordChannel channel;

            switch (consoleChannel)
            {
                case LogConsole.Commands:
                    channel = await this.dClient.GetChannelAsync(Channels.loggingChannel).ConfigureAwait(false);
                    break;

                case LogConsole.RoleEdits:
                    channel = await this.dClient.GetChannelAsync(Channels.loggingChannel).ConfigureAwait(false);
                    break;

                case LogConsole.UserInfo:
                    channel = await this.dClient.GetChannelAsync(Channels.loggingChannel).ConfigureAwait(false);
                    break;

                default:
                    channel = await this.dClient.GetChannelAsync(Channels.loggingChannel).ConfigureAwait(false);
                    break;
            }

            await channel.SendMessageAsync($"**[{DateTime.UtcNow}]** {DiscordEmoji.FromGuildEmote(this.dClient, emoji)} {message}").ConfigureAwait(false);
        }

        public async void Error(string message)
        {
            DiscordChannel logChannel = await this.dClient.GetChannelAsync(Channels.loggingChannel).ConfigureAwait(false);
            await logChannel.SendMessageAsync(message).ConfigureAwait(false);
        }
    }
}