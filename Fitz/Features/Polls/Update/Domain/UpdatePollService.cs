using Fitz.Features.Polls.Models;
using Fitz.Features.Polls.Update.Persistance;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Polls.Update.Domain
{
    /// <summary>
    /// Service for updating polls
    /// </summary>
    public class UpdatePollService
    {
        private readonly UpdatePollRepository _repository;

        public UpdatePollService(
            UpdatePollRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Updates a poll's status
        /// </summary>
        /// <param name="command">The command containing the update information</param>
        /// <returns>The updated poll</returns>
        public async Task<Poll> UpdatePollStatusAsync(UpdatePollCommand command)
        {
            try
            {
                // Get the poll from the database
                var poll = await _repository.GetPollByIdAsync(command.PollId);
                
                if (poll == null)
                {
                    throw new InvalidOperationException($"Poll with ID {command.PollId} not found");
                }

                // Check if the user is the poll creator
                if (poll.AccountId != command.UserId)
                {
                    throw new UnauthorizedAccessException("Only the poll creator can update the poll");
                }

                // Update the poll status
                poll.Status = command.Status;
                
                // If the poll is being closed or approved/declined, set the evaluation date
                if (command.Status == PollStatus.Closed || 
                    command.Status == PollStatus.Approved || 
                    command.Status == PollStatus.Declined)
                {
                    poll.EvaluatedOn = DateTime.UtcNow;
                }
                
                // Save the updated poll
                var updatedPoll = await _repository.UpdatePollAsync(poll);

                return updatedPoll;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
} 