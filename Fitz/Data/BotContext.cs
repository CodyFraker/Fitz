using Fitz.Features.Polls.Models;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Data
{
    /// <summary>
    /// Database context for the bot
    /// </summary>
    public class BotContext : DbContext
    {
        public BotContext(DbContextOptions<BotContext> options) : base(options)
        {
        }

        /// <summary>
        /// Polls in the database
        /// </summary>
        public DbSet<Poll> Polls { get; set; }

        /// <summary>
        /// Poll options in the database
        /// </summary>
        public DbSet<PollOptions> PollsOptions { get; set; }

        /// <summary>
        /// Votes in the database
        /// </summary>
        public DbSet<Vote> Votes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Poll entity
            modelBuilder.Entity<Poll>()
                .HasKey(p => p.Id);

            // Configure PollOptions entity
            modelBuilder.Entity<PollOptions>()
                .HasKey(po => po.Id);

            // Configure Vote entity
            modelBuilder.Entity<Vote>()
                .HasKey(v => v.Id);
        }
    }
} 