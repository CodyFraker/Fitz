using DSharpPlus;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Features.Accounts.Commands;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Features.Polls.Models;
using Fitz.Variables.Emojis;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fitz.Features.Polls.Create.Domain;
using Fitz.Features.Polls.Update.Domain;
using Fitz.Features.Polls.Vote.Domain;

namespace Fitz.Features.Polls
{
    /// <summary>
    /// Service for managing polls
    /// This class provides a facade for the domain services
    /// </summary>
    public class PollService
    {
        private readonly CreatePollService _createPollService;
        private readonly UpdatePollService _updatePollService;
        private readonly VoteService _voteService;
        private readonly IServiceScopeFactory _scopeFactory;

        public PollService(
            CreatePollService createPollService,
            UpdatePollService updatePollService,
            VoteService voteService,
            IServiceScopeFactory scopeFactory)
        {
            _createPollService = createPollService ?? throw new ArgumentNullException(nameof(createPollService));
            _updatePollService = updatePollService ?? throw new ArgumentNullException(nameof(updatePollService));
            _voteService = voteService ?? throw new ArgumentNullException(nameof(voteService));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        /// <summary>
        /// Creates a new poll
        /// </summary>
        /// <param name="accountId">The Discord user ID of the poll creator</param>
        /// <param name="channelId">The Discord channel ID where the poll should be posted</param>
        /// <param name="title">The title of the poll</param>
        /// <param name="description">The description of the poll</param>
        /// <param name="options">The options for the poll</param>
        /// <param name="pollType">The type of poll</param>
        /// <param name="endDate">The end date of the poll</param>
        /// <param name="allowMultipleVotes">Whether the poll allows multiple votes</param>
        /// <returns>The created poll</returns>
        public async Task<Poll> CreatePollAsync(
            ulong accountId,
            ulong channelId,
            string title,
            string description,
            List<string> options,
            PollType pollType,
            DateTime endDate,
            bool allowMultipleVotes)
        {
            var command = new CreatePollCommand(
                accountId,
                channelId,
                title,
                description,
                options,
                pollType,
                endDate,
                allowMultipleVotes);

            return await _createPollService.CreatePollAsync(command);
        }

        /// <summary>
        /// Updates a poll's status
        /// </summary>
        /// <param name="pollId">The ID of the poll to update</param>
        /// <param name="status">The new status for the poll</param>
        /// <param name="userId">The Discord user ID of the user updating the poll</param>
        /// <returns>The updated poll</returns>
        public async Task<Poll> UpdatePollStatusAsync(int pollId, PollStatus status, ulong userId)
        {
            var command = new UpdatePollCommand(pollId, status, userId);
            return await _updatePollService.UpdatePollStatusAsync(command);
        }

        /// <summary>
        /// Votes on a poll
        /// </summary>
        /// <param name="pollId">The ID of the poll to vote on</param>
        /// <param name="userId">The Discord user ID of the voter</param>
        /// <param name="optionIndex">The option index the user is voting for (0-based)</param>
        /// <returns>True if the vote was successful, false otherwise</returns>
        public async Task<bool> VoteAsync(int pollId, ulong userId, int optionIndex)
        {
            var command = new VoteCommand(pollId, userId, optionIndex);
            return await _voteService.VoteAsync(command);
        }

        /// <summary>
        /// For backward compatibility with the BankService
        /// </summary>
        public async Task UserSubmittedPollPenalty(ulong accountId)
        {
            // This method is kept for backward compatibility
            // The actual implementation is now in the BankService
        }

        /// <summary>
        /// Gets a poll by its message ID
        /// </summary>
        /// <param name="messageId">The Discord message ID of the poll</param>
        /// <returns>The poll, or null if not found</returns>
        public Poll GetPoll(ulong messageId)
        {
            using var scope = _scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return db.Polls.FirstOrDefault(p => p.MessageId == messageId);
        }

        /// <summary>
        /// Gets the options for a poll
        /// </summary>
        /// <param name="poll">The poll to get options for</param>
        /// <returns>A list of poll options</returns>
        public List<PollOptions> GetPollOptions(Poll poll)
        {
            using var scope = _scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return db.PollsOptions.Where(o => o.PollId == poll.Id).ToList();
        }

        /// <summary>
        /// Gets a vote by user on a specific poll
        /// </summary>
        /// <param name="poll">The poll to check</param>
        /// <param name="userId">The Discord user ID of the voter</param>
        /// <returns>The vote, or null if the user hasn't voted</returns>
        public object GetVoteByUserOnPoll(Poll poll, ulong userId)
        {
            using var scope = _scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return db.Votes.FirstOrDefault(v => v.PollId == poll.Id && v.UserId == userId);
        }

        /// <summary>
        /// Adds a vote to a poll
        /// </summary>
        /// <param name="poll">The poll to vote on</param>
        /// <param name="option">The option to vote for</param>
        /// <param name="account">The account that is voting</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task AddVote(Poll poll, PollOptions option, Account account)
        {
            using var scope = _scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

            var vote = new Models.Vote
            {
                PollId = poll.Id,
                UserId = account.Id,
                Choice = option.Id,
                Timestamp = DateTime.UtcNow
            };

            db.Votes.Add(vote);
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Updates a vote on a poll
        /// </summary>
        /// <param name="vote">The vote to update</param>
        /// <param name="optionId">The new option ID</param>
        /// <param name="account">The account that is voting</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task UpdateVote(object voteObj, int optionId, Account account)
        {
            using var scope = _scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

            var vote = voteObj as Models.Vote;
            if (vote == null)
            {
                return;
            }

            vote.Choice = optionId;
            vote.Timestamp = DateTime.UtcNow;

            db.Votes.Update(vote);
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Gets all polls submitted by a specific user
        /// </summary>
        /// <param name="userId">The Discord user ID of the submitter</param>
        /// <returns>A list of polls submitted by the user</returns>
        public IEnumerable<Poll> GetPollsSubmittedByUser(ulong userId)
        {
            using var scope = _scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return db.Polls.Where(p => p.AccountId == userId).ToList();
        }

        /// <summary>
        /// Adds options to a poll
        /// </summary>
        /// <param name="poll">The poll to add options to</param>
        /// <param name="options">The options to add</param>
        /// <returns>A result indicating success or failure</returns>
        public async Task<Result> AddPollOption(Poll poll, List<PollOptions> options)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                foreach (var option in options)
                {
                    option.PollId = poll.Id;
                    db.PollsOptions.Add(option);
                }

                await db.SaveChangesAsync();
                return new Result(true, "Poll options added successfully", poll);
            }
            catch (Exception ex)
            {
                return new Result(false, $"Failed to add poll options: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Adds a new poll to the database
        /// </summary>
        /// <param name="poll">The poll to add</param>
        /// <returns>A result indicating success or failure</returns>
        public async Task<Result> AddPoll(Poll poll)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                db.Polls.Add(poll);
                await db.SaveChangesAsync();
                return new Result(true, "Poll added successfully", poll);
            }
            catch (Exception ex)
            {
                return new Result(false, $"Failed to add poll: {ex.Message}", null);
            }
        }
    }
} 