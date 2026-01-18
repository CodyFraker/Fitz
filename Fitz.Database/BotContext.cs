using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pomelo.EntityFrameworkCore.MySql;

namespace Fitz.Database
{
    public class BotContext : DbContext
    {
        public BotContext()
        {
        }

        public BotContext(DbContextOptions<BotContext> options) : base(options)
        { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<BlackjackGame> BlackjackGame { get; set; }
        public DbSet<BlackjackPlayers> BlackjackPlayers { get; set; }
        public DbSet<Lottery> Drawing { get; set; }
        public DbSet<Ticket> Ticket { get; set; }
        public DbSet<Winners> Winners { get; set; }
        public DbSet<Poll> Polls { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<PollOptions> PollsOptions { get; set; }
        public DbSet<Renames> Renames { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<FeatureStatus> FeatureStatuses { get; set; }
        public DbSet<Settings> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>()
                .Property(s => s.Reason)
                .HasConversion(new EnumToStringConverter<Reason>());

            modelBuilder.Entity<Poll>()
                .Property(s => s.Type)
                .HasConversion(new EnumToStringConverter<PollType>());

            modelBuilder.Entity<Poll>()
                .Property(s => s.Status)
                .HasConversion(new EnumToStringConverter<PollStatus>());

            modelBuilder.Entity<BlackjackGame>()
                .Property(s => s.Type)
                .HasConversion(new EnumToStringConverter<GameType>());

            modelBuilder.Entity<BlackjackGame>()
                .Property(s => s.Status)
                .HasConversion(new EnumToStringConverter<BlackjackGameStatus>());

            modelBuilder.Entity<Renames>()
                .Property(s => s.Status)
                .HasConversion(new EnumToStringConverter<RenameStatus>());

            base.OnModelCreating(modelBuilder);
        }
    }
}
