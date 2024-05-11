using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Core.Contexts;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Features.Polls.Models;
using Fitz.Variables.Emojis;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Polls
{
    public class PollService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private AccountService accountService;
        private BankService bankService;

        public PollService(IServiceScopeFactory scopeFactory, AccountService accountService, BankService bankService)
        {
            this.scopeFactory = scopeFactory;
            this.accountService = accountService;
            this.bankService = bankService;
        }

        public async Task<Poll> AddPoll(Poll poll)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            db.Polls.Add(poll);
            await db.SaveChangesAsync();
            return poll;
        }

        public Poll GetPoll(ulong messageId)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return db.Polls.Where((x) => x.MessageId == messageId).FirstOrDefault();
        }

        public async Task<bool> isPoll(ulong messageId)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return db.Polls.Where((x) => x.MessageId == messageId).FirstOrDefault() != null;
        }

        /// <summary>
        /// Returns all votes that were made to a particular poll.
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public async Task<List<Vote>> GetVotesOnPoll(ulong messageId)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
            Poll poll = db.Polls.Where((x) => x.MessageId == messageId).FirstOrDefault();

            return db.Votes.Where(v => v.PollId == poll.Id).ToList();
        }

        /// <summary>
        /// Returns a vote by a user if they have voted on a poll.
        /// </summary>
        /// <param name="messageId">Discord Message ID of the poll</param>
        /// <param name="userId">Discord User ID who added the reaction</param>
        /// <returns>Vote</returns>
        public async Task<Vote> GetVoteByUserOnPoll(Poll poll, ulong userId)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return db.Votes.FirstOrDefault(v => v.PollId == poll.Id && v.UserId == userId);
        }

        public async Task AddVote(Poll poll, PollOptions option, Account account)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            // Check to see if the user has already voted on this poll before adding a new vote.
            if (db.Votes.Any(v => v.PollId == poll.Id && v.UserId == account.Id))
            {
                return;
            }

            Vote vote = new Vote
            {
                PollId = poll.Id,
                Choice = option.Id,
                UserId = account.Id,
                Timestamp = DateTime.Now
            };

            db.Votes.Add(vote);
            await db.SaveChangesAsync();
        }

        public async Task UpdateVote(Vote vote, int newOptionId, Account account)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            vote.Choice = newOptionId;
            vote.Timestamp = DateTime.Now;

            db.Update(vote);

            await db.SaveChangesAsync();
        }

        public async Task<bool> HasUserVoted(ulong messageId, ulong userId)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
            Poll poll = db.Polls.Where((x) => x.MessageId == messageId).FirstOrDefault();

            return db.Votes.Any(v => v.PollId == poll.Id && v.UserId == userId);
        }

        public bool IsValidPollEmoji(Poll poll, DiscordEmoji emoji)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            if (emoji.Id == 0)
            {
                return db.PollsOptions.Any(p => p.PollId == poll.Id && p.Name == emoji.Name);
            }

            return db.PollsOptions.Any(p => p.PollId == poll.Id && p.EmojiId == emoji.Id);
        }

        public async Task AddPollOption(Poll poll, List<DiscordEmoji> options)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            foreach (DiscordEmoji option in options)
            {
                PollOptions pollOption = new PollOptions
                {
                    PollId = poll.Id,
                    Name = option.Name,
                    EmojiId = option.Id
                };

                db.PollsOptions.Add(pollOption);
            }

            await db.SaveChangesAsync();
        }

        public List<PollOptions> GetValidPollOptions(Poll poll)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return db.PollsOptions.Where(p => p.PollId == poll.Id).ToList();
        }

        public List<Poll> GetPolls()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return db.Polls.ToList();
        }
    }
}