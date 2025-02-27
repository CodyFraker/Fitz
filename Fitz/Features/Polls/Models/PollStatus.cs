using DSharpPlus.SlashCommands;

namespace Fitz.Features.Polls.Models
{
    /// <summary>
    /// Represents the status of a poll
    /// </summary>
    public enum PollStatus
    {
        /// <summary>
        /// The poll is pending approval
        /// </summary>
        Pending = 0,

        /// <summary>
        /// The poll has been approved
        /// </summary>
        Approved = 1,

        /// <summary>
        /// The poll has been declined
        /// </summary>
        Declined = 2,

        /// <summary>
        /// The poll is active and accepting votes
        /// </summary>
        Active = 3,

        /// <summary>
        /// The poll is closed and no longer accepting votes
        /// </summary>
        Closed = 4
    }
}