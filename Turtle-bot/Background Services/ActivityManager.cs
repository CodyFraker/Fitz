namespace Fitz.BackgroundServices
{
    using System;
    using System.Net.WebSockets;
    using System.Threading.Tasks;
    using Fitz.Variables;
    using DSharpPlus;
    using DSharpPlus.Entities;

    /// <summary>
    /// Manages Fitz's activity and streaming statuses.
    /// </summary>
    public class ActivityManager
    {
        private const int AutoResetMs = 3000;
        private const string DefaultActivity = "you buy me a beer.";

        private readonly DiscordClient dClient;

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
            await this.ResetActivityAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Resets Fitz's activity to <see cref="DefaultActivity"/>.
        /// </summary>
        /// <returns>Awaitable task.</returns>
        public Task ResetActivityAsync() => this.TrySetActivityAsync(DefaultActivity);

        /// <summary>
        /// Sets Fitz's activity if not currently streaming.
        /// </summary>
        /// <param name="activity">Activity description.</param>
        /// <param name="activityType">Activity type.</param>
        /// <param name="autoReset">Automatically switch back to <see cref="DefaultActivity"/> after <see cref="AutoResetMs"/>.</param>
        /// <returns>Awaitable task.</returns>
        public async Task TrySetActivityAsync(string activity, ActivityType activityType = ActivityType.Watching, bool autoReset = false)
        {
            await this.dClient.UpdateStatusAsync(new DiscordActivity
            {
                ActivityType = activityType,
                Name = activity,
            }).ConfigureAwait(false);

            if (autoReset)
            {
                await Task.Delay(AutoResetMs).ConfigureAwait(false);
                await this.ResetActivityAsync().ConfigureAwait(false);
            }
        }
    }
}
