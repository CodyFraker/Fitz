using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.ModalCommands;
using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Features.Polls.Models;
using Fitz.Variables.Emojis;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Polls
{
    public class PollService(IServiceScopeFactory scopeFactory, AccountService accountService, BankService bankService, BotLog botLog)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly AccountService accountService = accountService;
        private readonly BankService bankService = bankService;
        private readonly BotLog botLog = botLog;

        #region Add Poll

        public async Task<Result> AddPoll(Poll poll)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                db.Polls.Add(poll);
                await db.SaveChangesAsync();

                await this.bankService.UserSubmittedPollPenalty(poll.AccountId);
                this.botLog.Information(LogConsoleSettings.PollLog, PollEmojis.InfoIcon, $"User {poll.AccountId} submitted a new poll {poll.Id}");
                return new Result(true, $"Poll #{poll.Id} created.", poll);
            }
            catch (Exception e)
            {
                return new Result(false, e.Message, null);
            }
        }

        #endregion Add Poll

        #region Evaluate Poll

        public async Task<Result> EvaluatePoll(Poll pendingPoll, PollStatus evaluation)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                pendingPoll.Status = evaluation;
                pendingPoll.EvaluatedOn = DateTime.Now;
                db.Polls.Update(pendingPoll);
                await db.SaveChangesAsync();

                if (evaluation == PollStatus.Approved)
                {
                    await this.bankService.AwardPollApproval(pendingPoll.AccountId);
                }
                else
                {
                    await this.bankService.DeclineUserPoll(pendingPoll.AccountId);
                }
                this.botLog.Information(LogConsoleSettings.PollLog, PollEmojis.InfoIcon, $"Poll {pendingPoll.Id} state was set to {evaluation}");
                return new Result(true, $"Pending Poll #{pendingPoll.Id} approved.", pendingPoll);
            }
            catch (Exception e)
            {
                this.botLog.Information(LogConsoleSettings.PollLog, PollEmojis.InfoIcon, $"Poll {pendingPoll.Id} state update failed.");
                return new Result(false, e.Message, null);
            }
        }

        #endregion Evaluate Poll

        #region GET

        /// <summary>
        /// Return a poll from a Discord Message ID
        /// </summary>
        /// <param name="messageId">Discord Message ID</param>
        /// <returns>Poll</returns>
        public Poll GetPoll(ulong messageId)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return db.Polls.Where((x) => x.MessageId == messageId).FirstOrDefault();
        }

        /// <summary>
        /// Returns all votes that were made to a particular poll.
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public List<Vote> GetVotesOnPoll(ulong messageId)
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
        public Vote GetVoteByUserOnPoll(Poll poll, ulong userId)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return db.Votes.FirstOrDefault(v => v.PollId == poll.Id && v.UserId == userId);
        }

        /// <summary>
        /// Get a list of polls submitted by a particular user.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="approved"></param>
        /// <returns></returns>
        public List<Poll> GetPollsSubmittedByUser(ulong accountId, bool? approved)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return [.. db.Polls.Where(p => p.AccountId == accountId && p.Status == PollStatus.Approved)];
        }

        public int GetTotalApprovedPolls()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return db.Polls.Where(p => p.Status == PollStatus.Approved).Count();
        }

        public List<PollOptions> GetPollOptions(Poll poll)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return [.. db.PollsOptions.Where(pollOptions => pollOptions.PollId == poll.Id)];
        }

        public List<Poll> GetPolls()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return db.Polls.ToList();
        }

        #endregion GET

        public bool isPoll(ulong messageId)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return db.Polls.Where((x) => x.MessageId == messageId).FirstOrDefault() != null;
        }

        #region Add Vote to Poll

        public async Task AddVote(Poll poll, PollOptions option, Account account)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            // Check to see if the user has already voted on this poll before adding a new vote.
            if (db.Votes.Any(v => v.PollId == poll.Id && v.UserId == account.Id))
            {
                return;
            }

            Vote vote = new()
            {
                PollId = poll.Id,
                Choice = option.Id,
                UserId = account.Id,
                Timestamp = DateTime.Now
            };

            // Add user vote to the database.
            db.Votes.Add(vote);
            await db.SaveChangesAsync();

            // Add beer to the user's account for voting.
            await this.bankService.AwardPollVote(account.Id);
            this.botLog.Information(LogConsoleSettings.PollLog, PollEmojis.InfoIcon, $"User {account.Id} voted on poll {poll.Id}");

            // Add beer to the poll creator's account for having their poll voted on.
            if (poll.AccountId != account.Id)
            {
                await this.bankService.TipPollCreatorVote(poll.AccountId);
                this.botLog.Information(LogConsoleSettings.PollLog, PollEmojis.InfoIcon, $"User {account.Id} voted on poll {poll.Id} and tipped the poll creator {poll.AccountId}");
            }
        }

        public async Task AddVote(Poll poll, PollOptions option, ulong accountId)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
            Account account = accountService.FindAccount(accountId);

            await this.AddVote(poll, option, account);
        }

        #endregion Add Vote to Poll

        #region Update Polls and Votes

        public async Task UpdateVote(Vote vote, int newOptionId, Account account)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            vote.Choice = newOptionId;
            vote.Timestamp = DateTime.Now;

            db.Update(vote);

            await db.SaveChangesAsync();
        }

        public async Task<Result> UpdatePollAsync(Poll poll)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                db.Polls.Update(poll);
                await db.SaveChangesAsync();
                return new Result(true, $"Poll #{poll.Id} updated.", poll);
            }
            catch (Exception e)
            {
                return new Result(false, e.Message, null);
            }
        }

        #endregion Update Polls and Votes

        public bool HasUserVoted(ulong messageId, ulong userId)
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
                return db.PollsOptions.Any(p => p.PollId == poll.Id && p.EmojiName == emoji.Name);
            }

            return db.PollsOptions.Any(p => p.PollId == poll.Id && p.EmojiId == emoji.Id);
        }

        public async Task<Result> AddPollOption(Poll poll, List<PollOptions> pollOptions)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                foreach (PollOptions option in pollOptions)
                {
                    option.PollId = poll.Id;
                    db.PollsOptions.Add(option);
                }
                await db.SaveChangesAsync();
                return new Result(true, $"Poll #{poll.Id} options added.", pollOptions);
            }
            catch (Exception e)
            {
                return new Result(false, e.Message, null);
            }
        }

        public List<Poll> GetPollsSubmittedByUser(ulong accountId)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return [.. db.Polls.Where(p => p.AccountId == accountId)];
        }

        public DiscordEmbed GeneratePollEmbed(DiscordClient dClient, Poll poll, List<PollOptions> pollOptions)
        {
            // Set base embed color to white.
            DiscordColor embedColor = new DiscordColor(250, 250, 250);

            // Set description to empty string.
            string description = string.Empty;
            foreach (PollOptions option in pollOptions)
            {
                // If built in emoji
                if (option.EmojiId == 0)
                {
                    description += $"{DiscordEmoji.FromName(dClient, option.EmojiName)} **{option.Answer}**\n";
                }
                else if (option.EmojiId != 0 && option.EmojiId != null)
                {
                    // If custom emoji
                    description += $"{DiscordEmoji.FromGuildEmote(dClient, option.EmojiId.Value)} **{option.Answer}**\n";
                }
            }

            switch (poll.Type)
            {
                case PollType.Number:
                    embedColor = new DiscordColor(PollEmbedColors.NumberPoll);
                    break;

                case PollType.Color:
                    embedColor = new DiscordColor(PollEmbedColors.ColorPoll);
                    break;

                case PollType.YesOrNo:
                    embedColor = new DiscordColor(PollEmbedColors.YesNoPoll);
                    break;

                case PollType.ThisOrThat:
                    embedColor = new DiscordColor(PollEmbedColors.ThisOrThatPoll);
                    break;

                case PollType.HotTake:
                    embedColor = new DiscordColor(PollEmbedColors.HottakePoll);
                    break;
            }

            DiscordEmbed pollEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(dClient, PollEmojis.InfoIcon).Url,
                    Text = $"Poll #{poll.Id} | {poll.Type}",
                },
                Color = embedColor,
                Timestamp = DateTime.UtcNow,
                Title = $"__{poll.Question}__",
                Description = description,
            };

            return pollEmbed;
        }

        public DiscordEmbed UpdatePollEmbed(DiscordClient dClient, Poll poll, List<PollOptions> pollOptions, DiscordMessage? pollMessage)
        {
            // Set base embed color to white.
            DiscordColor embedColor = new(250, 250, 250);

            if (poll.Status == PollStatus.Approved)
            {
                embedColor = new DiscordColor(PollEmbedColors.ApprovedPoll);
            }
            if (poll.Status == PollStatus.Declined)
            {
                embedColor = new DiscordColor(PollEmbedColors.DeclinedPoll);
            }

            // Set description to empty string.
            string description = string.Empty;
            foreach (PollOptions option in pollOptions)
            {
                // If built in emoji
                if (option.EmojiId == 0)
                {
                    description += $"{DiscordEmoji.FromName(dClient, option.EmojiName)} **{option.Answer}**\n";
                }
                else if (option.EmojiId != 0 && option.EmojiId != null)
                {
                    // If custom emoji
                    description += $"{DiscordEmoji.FromGuildEmote(dClient, option.EmojiId.Value)} **{option.Answer}**\n";
                }
            }

            string embedTitle = string.Empty;
            if (poll.Status == PollStatus.Approved)
            {
                embedTitle = $"~~__{poll.Question}__~~ - Approved";
            }
            if (poll.Status == PollStatus.Declined)
            {
                embedTitle = $"~~__{poll.Question}__~~ - Denied";
            }

            if (pollMessage != null)
            {
                description += $"\nView Poll -> {pollMessage.JumpLink}\n";
            }

            DiscordEmbed pollEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(dClient, PollEmojis.InfoIcon).Url,
                    Text = $"Poll #{poll.Id} | {poll.Type}",
                },
                Color = embedColor,
                Timestamp = DateTime.UtcNow,
                Title = embedTitle,
                Description = description,
            };

            return pollEmbed;
        }
    }
}