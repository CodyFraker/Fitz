using Fitz.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Shared.Data
{
    public class BotContext : DbContext
    {
        public BotContext(DbContextOptions<BotContext> options) : base(options)
        {
        }

        // Define DbSets for the tables
        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<Lottery> Lotteries { get; set; } = null!;
        public DbSet<LotteryEntry> LotteryEntries { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<LotteryEntry>()
                .HasOne(e => e.Lottery)
                .WithMany(l => l.Entries)
                .HasForeignKey(e => e.LotteryId);
        }
    }
} 