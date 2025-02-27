using Fitz.Core.Contexts;
using Fitz.Features.Polls.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Polls.Update.Persistance
{
    /// <summary>
    /// Repository for updating polls
    /// </summary>
    public class UpdatePollRepository
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public UpdatePollRepository(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        /// <summary>
        /// Gets a poll by its ID
        /// </summary>
        /// <param name="pollId">The ID of the poll to get</param>
        /// <returns>The poll, or null if not found</returns>
        public async Task<Poll> GetPollByIdAsync(int pollId)
        {
            using var scope = _scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return await db.Polls.FindAsync(pollId);
        }

        /// <summary>
        /// Updates a poll in the database
        /// </summary>
        /// <param name="poll">The poll to update</param>
        /// <returns>The updated poll</returns>
        public async Task<Poll> UpdatePollAsync(Poll poll)
        {
            using var scope = _scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

            db.Polls.Update(poll);
            await db.SaveChangesAsync();

            return poll;
        }
    }
} 