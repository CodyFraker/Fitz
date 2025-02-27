using Fitz.Core.Discord;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Features.Polls.Create.Persistance;
using Fitz.Features.Polls.Models;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fitz.Features.Polls.Create.Domain
{
    /// <summary>
    /// Service for creating polls
    /// </summary>
    public class CreatePollService
    {
        private readonly CreatePollRepository _repository;
        private readonly BankService _bankService;
        private readonly BotLog _botLog;

        public CreatePollService(
            CreatePollRepository repository,
            BankService bankService,
            BotLog botLog)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _bankService = bankService ?? throw new ArgumentNullException(nameof(bankService));
            _botLog = botLog ?? throw new ArgumentNullException(nameof(botLog));
        }

        /// <summary>
        /// Creates a new poll
        /// </summary>
        /// <param name="command">The command containing the poll information</param>
        /// <returns>The created poll</returns>
        public async Task<Poll> CreatePollAsync(CreatePollCommand command)
        {
            try
            {
                // Create the poll entity
                var poll = new Poll
                {
                    AccountId = command.AccountId,
                    ChannelId = command.ChannelId,
                    Title = command.Title,
                    Description = command.Description,
                    Type = command.PollType,
                    EndDate = command.EndDate,
                    AllowMultipleVotes = command.AllowMultipleVotes,
                    Status = PollStatus.Active,
                    CreatedOn = DateTime.UtcNow
                };

                // Create the poll options
                var pollOptions = new PollOptions
                {
                    Values = command.Options
                };

                poll.Options = pollOptions;

                // Save the poll to the database
                var createdPoll = await _repository.AddPollAsync(poll);

                // Apply the poll submission penalty
                await _bankService.UserSubmittedPollPenalty(command.AccountId);

                // Log the poll creation
                _botLog.Information(LogConsoleSettings.PollLog, PollEmojis.InfoIcon,
                    $"User {poll.AccountId} submitted a new poll {poll.Id}");

                return createdPoll;
            }
            catch (Exception ex)
            {
                _botLog.Information(LogConsoleSettings.PollLog, PollEmojis.InfoIcon,
                    $"Error creating poll: {ex.Message}");
                throw;
            }
        }
    }
}