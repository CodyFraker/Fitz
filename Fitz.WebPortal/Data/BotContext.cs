using Microsoft.EntityFrameworkCore;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank.Models;
using Fitz.Features.Lottery.Models;

namespace Fitz.WebPortal.Data
{
    public class WebPortalContext : DbContext
    {
        public WebPortalContext(DbContextOptions<WebPortalContext> options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<Lottery> Lotteries { get; set; } = null!;
        public DbSet<Ticket> Tickets { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Account>()
                .HasKey(a => a.Id);

            modelBuilder.Entity<Transaction>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<Lottery>()
                .HasKey(l => l.Id);

            modelBuilder.Entity<Ticket>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<Lottery>()
                .HasMany(l => l.Tickets)
                .WithOne()
                .HasForeignKey(t => t.LotteryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 