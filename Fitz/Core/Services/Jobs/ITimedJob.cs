namespace Fitz.Core.Services.Jobs
{
    using System.Threading.Tasks;

    public interface ITimedJob
    {
        /// <summary>
        /// Discord Emoji used for logging in particular discord channels.
        /// </summary>
        ulong Emoji { get; }

        /// <summary>
        /// Provide a Cron Expression which defines the interval at which the job should run.
        /// Examples:
        /// Every 5 Minutes: "*/5 * * * *"
        /// Every Hour: "0 0 * * *"
        /// Every 5 Hours: "0 */5 * * *"
        /// Noon Everyday: "0 12 * * *"
        /// </summary>
        string Interval { get; }

        Task Execute();
    }
}