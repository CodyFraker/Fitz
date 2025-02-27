using Fitz.Features.Polls.Models;
using Fitz.Features.Polls.Vote.Persistance;
using Fitz.Variables.Emojis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Polls.Vote.Domain
{
    /// <summary>
    /// Service for handling voting on polls
    /// </summary>
    public class VoteService
    {
        private readonly VoteRepository _voteRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="VoteService"/> class.
        /// </summary>
        /// <param name="voteRepository">The vote repository.</param>
        public VoteService(VoteRepository voteRepository)
        {
            _voteRepository = voteRepository ?? throw new ArgumentNullException(nameof(voteRepository));
        }

        /// <summary>
        /// Votes on a poll
        /// </summary>
        /// <param name="command">The vote command.</param>
        /// <returns>True if the vote was successful, false otherwise.</returns>
        public async Task<bool> VoteAsync(VoteCommand command)
        {
            // Get the poll
            var poll = await _voteRepository.GetPollByIdAsync(command.PollId);
            if (poll == null)
            {
                return false;
            }

            // Check if the poll is active
            if (poll.Status != PollStatus.Active)
            {
                return false;
            }

            // Check if the poll has ended
            if (DateTime.UtcNow > poll.EndDate)
            {
                return false;
            }

            // Check if the option index is valid
            if (command.OptionIndex < 0 || command.OptionIndex >= poll.Options.Values.Count)
            {
                return false;
            }

            // Check if the user has already voted
            var existingVote = await _voteRepository.GetVoteAsync(command.PollId, command.UserId);
            if (existingVote != null && !poll.AllowMultipleVotes)
            {
                return false;
            }

            // Create a new vote
            var vote = new Models.Vote
            {
                PollId = command.PollId,
                UserId = command.UserId,
                Choice = command.OptionIndex,
                Timestamp = DateTime.UtcNow
            };

            // Add the vote
            await _voteRepository.AddVoteAsync(vote);

            return true;
        }
    }
} 