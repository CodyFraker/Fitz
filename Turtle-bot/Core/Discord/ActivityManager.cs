using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Variables.Channels;
using Fitz.Variables;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Fitz.Core.Discord
{
    public class ActivityManager
    {
        private const int AutoResetMs = 3000;
        private const string DefaultActivity = "beer.";

        private readonly DiscordClient dClient;

        private ulong streamOwnerID;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityManager"/> class.
        /// </summary>
        /// <param name="dClient">Discord socket client.</param>
        public ActivityManager(DiscordClient dClient)
        {
            this.dClient = dClient;
        }

        /// <summary>
        /// Clears the stream and returns to <see cref="DefaultActivity"/>.
        /// </summary>
        /// <returns>Awaitable task.</returns>
        public async Task ClearStreamAsync()
        {
            this.streamOwnerID = 0;
            await this.ResetActivityAsync();
        }

        /// <summary>
        /// Checks whether a user is the current stream's owner.
        /// </summary>
        /// <param name="streamerID">Discord user id.</param>
        /// <returns>Boolean.</returns>
        public bool IsStreamOwner(ulong streamerID)
        {
            return this.streamOwnerID == streamerID;
        }

        /// <summary>
        /// Resets Bloon's activity to <see cref="DefaultActivity"/>.
        /// </summary>
        /// <returns>Awaitable task.</returns>
        public Task ResetActivityAsync()
        {
            return this.TrySetActivityAsync(DefaultActivity, DiscordActivityType.Watching);
        }

        /// <summary>
        /// Advertises/Shares a stream. '<paramref name="force"/>' overrides any current stream.
        /// </summary>
        /// <param name="streamerID">Discord user id.</param>
        /// <param name="streamerName">Stream name.</param>
        /// <param name="url">Stream url.</param>
        /// <param name="force">Force Bloon to switch to this stream.</param>
        /// <returns>Awaitable task.</returns>
#pragma warning disable CA1054 // Uri parameters should not be strings

        public async Task SetStreamAsync(ulong streamerID, string streamerName, string url, bool force = false)
#pragma warning restore CA1054 // Uri parameters should not be strings
        {
            if (!force && this.dClient.CurrentUser.Presence.Activity.ActivityType == DiscordActivityType.Streaming)
            {
                return;
            }

            DiscordChannel sbgGeneral = await this.dClient.GetChannelAsync(Channels.loggingChannel);
            DiscordEmbed streamEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = url,
                    IconUrl = DiscordEmoji.FromGuildEmote(this.dClient, Emoji.Run).Url,
                },
                Description = $"{streamerName} is streaming Intruder! Stop by and check them out! [**{streamerName}'s Stream**]({url})",
                Color = new DiscordColor(100, 64, 165),
                Timestamp = DateTime.UtcNow,
                Title = $"*Stream Detected!*",
            };

            this.streamOwnerID = streamerID;
            await this.dClient.UpdateStatusAsync(new DiscordActivity
            {
                ActivityType = DiscordActivityType.Streaming,
                Name = $"Intruder with {streamerName}",
                StreamUrl = url,
            });

            await sbgGeneral.SendMessageAsync(embed: streamEmbed);
        }

        /// <summary>
        /// Sets Bloon's activity if not currently streaming.
        /// </summary>
        /// <param name="activity">Activity description.</param>
        /// <param name="activityType">Activity type.</param>
        /// <param name="autoReset">Automatically switch back to <see cref="DefaultActivity"/> after <see cref="AutoResetMs"/>.</param>
        /// <returns>Awaitable task.</returns>
        public async Task TrySetActivityAsync(string activity, DiscordActivityType activityType, bool autoReset = false)
        {
            if ((this.dClient.CurrentUser.Presence.Activity.ActivityType == DiscordActivityType.Streaming && activity != DefaultActivity)
                || Bot.SocketState != WebSocketState.Open)
            {
                return;
            }

            await this.dClient.UpdateStatusAsync(new DiscordActivity
            {
                ActivityType = activityType,
                Name = activity,
            });

            if (autoReset)
            {
                await Task.Delay(AutoResetMs);
                await this.ResetActivityAsync();
            }
        }
    }
}