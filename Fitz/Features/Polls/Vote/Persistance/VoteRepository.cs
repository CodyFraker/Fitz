using Fitz.Data;
using Fitz.Features.Polls.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Polls.Vote.Persistance
{
    /// <summary>
    /// Repository for handling votes on polls
    /// </summary>
    public class VoteRepository
    {
        private readonly IServiceScopeFactory _scopeFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="VoteRepository"/> class.
        /// </summary>
        /// <param name="scopeFactory">The service scope factory.</param>
        public VoteRepository(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        /// <summary>
        /// Gets a poll by its ID
        /// </summary>
        /// <param name="pollId">The poll ID.</param>
        /// <returns>The poll, or null if not found.</returns>
        public async Task<Poll> GetPollByIdAsync(int pollId)
        {
            using var scope = _scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

            var poll = await db.Polls
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == pollId);

            if (poll != null)
            {
                // Load the poll options
                poll.Options = await db.PollsOptions
                    .FirstOrDefaultAsync(o => o.PollId == pollId);
            }

            return poll;
        }

        /// <summary>
        /// Gets a vote by poll ID and user ID
        /// </summary>
        /// <param name="pollId">The poll ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>The vote, or null if not found.</returns>
        public async Task<Models.Vote> GetVoteAsync(int pollId, ulong userId)
        {
            using var scope = _scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return await db.Votes
                .FirstOrDefaultAsync(v => v.PollId == pollId && v.UserId == userId);
        }

        /// <summary>
        /// Adds a vote to the database
        /// </summary>
        /// <param name="vote">The vote to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddVoteAsync(Models.Vote vote)
        {
            using var scope = _scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

            db.Votes.Add(vote);
            await db.SaveChangesAsync();
        }
    }
} 