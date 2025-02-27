using System;

namespace Fitz.Features.Polls.Vote.Domain
{
    /// <summary>
    /// Command to vote on a poll
    /// </summary>
    public class VoteCommand
    {
        /// <summary>
        /// The ID of the poll to vote on
        /// </summary>
        public int PollId { get; }

        /// <summary>
        /// The Discord user ID of the voter
        /// </summary>
        public ulong UserId { get; }

        /// <summary>
        /// The option index the user is voting for (0-based)
        /// </summary>
        public int OptionIndex { get; }

        public VoteCommand(int pollId, ulong userId, int optionIndex)
        {
            if (pollId <= 0)
            {
                throw new ArgumentException("Poll ID must be greater than zero", nameof(pollId));
            }

            if (optionIndex < 0)
            {
                throw new ArgumentException("Option index must be non-negative", nameof(optionIndex));
            }

            PollId = pollId;
            UserId = userId;
            OptionIndex = optionIndex;
        }
    }
} 