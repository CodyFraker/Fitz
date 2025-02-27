using Fitz.Features.Polls.Models;
using System;

namespace Fitz.Features.Polls.Update.Domain
{
    /// <summary>
    /// Command to update a poll's status
    /// </summary>
    public class UpdatePollCommand
    {
        /// <summary>
        /// The ID of the poll to update
        /// </summary>
        public int PollId { get; }

        /// <summary>
        /// The new status for the poll
        /// </summary>
        public PollStatus Status { get; }

        /// <summary>
        /// The Discord user ID of the user updating the poll
        /// </summary>
        public ulong UserId { get; }

        public UpdatePollCommand(int pollId, PollStatus status, ulong userId)
        {
            if (pollId <= 0)
            {
                throw new ArgumentException("Poll ID must be greater than zero", nameof(pollId));
            }

            PollId = pollId;
            Status = status;
            UserId = userId;
        }
    }
} 