using Fitz.Features.Polls.Models;
using System;
using System.Collections.Generic;

namespace Fitz.Features.Polls.Create.Domain
{
    /// <summary>
    /// Command to create a new poll
    /// </summary>
    public class CreatePollCommand
    {
        /// <summary>
        /// The Discord user ID of the poll creator
        /// </summary>
        public ulong AccountId { get; }

        /// <summary>
        /// The Discord channel ID where the poll should be posted
        /// </summary>
        public ulong ChannelId { get; }

        /// <summary>
        /// The title of the poll
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// The description of the poll
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The options for the poll
        /// </summary>
        public List<string> Options { get; }

        /// <summary>
        /// The type of poll
        /// </summary>
        public PollType PollType { get; }

        /// <summary>
        /// The end date of the poll
        /// </summary>
        public DateTime EndDate { get; }

        /// <summary>
        /// Whether the poll allows multiple votes
        /// </summary>
        public bool AllowMultipleVotes { get; }

        public CreatePollCommand(
            ulong accountId,
            ulong channelId,
            string title,
            string description,
            List<string> options,
            PollType pollType,
            DateTime endDate,
            bool allowMultipleVotes)
        {
            AccountId = accountId;
            ChannelId = channelId;
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Options = options ?? throw new ArgumentNullException(nameof(options));
            PollType = pollType;
            EndDate = endDate;
            AllowMultipleVotes = allowMultipleVotes;

            if (options.Count < 2)
            {
                throw new ArgumentException("A poll must have at least 2 options", nameof(options));
            }
        }
    }
} 