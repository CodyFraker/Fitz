using Microsoft.EntityFrameworkCore;
using System;

namespace Fitz.WebPortal.Data
{
    public class BotContext : DbContext
    {
        public BotContext(DbContextOptions<BotContext> options) : base(options)
        {
        }

        // Define DbSets for the tables you need to access
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Lottery> Lotteries { get; set; }
        public DbSet<LotteryTicket> LotteryTickets { get; set; }
        public DbSet<LotteryWinner> LotteryWinners { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure entity mappings if needed
            modelBuilder.Entity<Account>().ToTable("accounts");
            modelBuilder.Entity<Transaction>().ToTable("transactions");
            modelBuilder.Entity<Lottery>().ToTable("lotteries");
            modelBuilder.Entity<LotteryTicket>().ToTable("lottery_tickets");
            modelBuilder.Entity<LotteryWinner>().ToTable("lottery_winners");
        }
    }

    // Define entity classes that match your database schema
    public class Account
    {
        public ulong Id { get; set; }
        public string Username { get; set; }
        public int Beer { get; set; }
        public int LifetimeBeer { get; set; }
        public int SafeBalance { get; set; }
        public int Favorability { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastSeenDate { get; set; }
        public DateTime LastActivityDate { get; set; }
        public bool SubscribeToLottery { get; set; }
        public int SubscribeTickets { get; set; }
        public bool Deactivated { get; set; }
    }

    public class Transaction
    {
        public int Id { get; set; }
        public ulong SenderId { get; set; }
        public ulong RecipientId { get; set; }
        public int Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public string Reason { get; set; }
        public int Type { get; set; }
    }

    public class Lottery
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Pool { get; set; }
        public bool CurrentLottery { get; set; }
        public int? WinningTicketId { get; set; }
    }

    public class LotteryTicket
    {
        public int Id { get; set; }
        public int LotteryId { get; set; }
        public ulong UserId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int TicketNumber { get; set; }
        public bool IsWinner { get; set; }
    }

    public class LotteryWinner
    {
        public int Id { get; set; }
        public int LotteryId { get; set; }
        public int TicketId { get; set; }
        public ulong UserId { get; set; }
        public int PrizeAmount { get; set; }
        public DateTime WinDate { get; set; }
        public bool PrizeClaimed { get; set; }
        public DateTime? ClaimDate { get; set; }
    }
} 