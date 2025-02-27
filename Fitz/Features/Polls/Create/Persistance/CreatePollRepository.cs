using Fitz.Core.Contexts;
using Fitz.Features.Polls.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Polls.Create.Persistance
{
    /// <summary>
    /// Repository for creating polls
    /// </summary>
    public class CreatePollRepository
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public CreatePollRepository(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        /// <summary>
        /// Adds a new poll to the database
        /// </summary>
        /// <param name="poll">The poll to add</param>
        /// <returns>The created poll with its ID</returns>
        public async Task<Poll> AddPollAsync(Poll poll)
        {
            using var scope = _scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

            db.Polls.Add(poll);
            await db.SaveChangesAsync();

            return poll;
        }
    }
} 