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

        public DbSet<AccountEntity> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<BlackjackGame> BlackjackGame { get; set; }
        public DbSet<BlackjackPlayers> BlackjackPlayers { get; set; }
        public DbSet<LotteryEntity> Drawing { get; set; }
        public DbSet<TicketEntity> Ticket { get; set; }
        public DbSet<WinnersEntity> Winners { get; set; }
        public DbSet<PollEntity> Polls { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<PollOptionsEntity> PollsOptions { get; set; }
        public DbSet<Renames> Renames { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<FeatureStatus> FeatureStatuses { get; set; }
        public DbSet<SettingsEntity> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>()
                .Property(s => s.Reason)
                .HasConversion(new EnumToStringConverter<Reason>());

            modelBuilder.Entity<PollEntity>()
                .Property(s => s.Type)
                .HasConversion(new EnumToStringConverter<PollTypeEnum>());

            modelBuilder.Entity<PollEntity>()
                .Property(s => s.Status)
                .HasConversion(new EnumToStringConverter<PollStatusEnum>());

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
