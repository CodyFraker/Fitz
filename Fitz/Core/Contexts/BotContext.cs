namespace Fitz.Core.Contexts
{
    using Fitz.Core.Models;
    using Fitz.Core.Services.Features;
    using Fitz.Core.Services.Jobs;
    using Fitz.Features.Bank.Models;
    using Fitz.Features.Polls.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using Newtonsoft.Json;
    using System;

    public partial class BotContext(DbContextOptions<BotContext> options) : DbContext(options)
    {
        /// <summary>
        /// Pulls variables stored in the .env file.
        /// </summary>
        public static string ConnectionString =>
            $"Host={Environment.GetEnvironmentVariable("DB_HOST")};"
            + $"Port={Environment.GetEnvironmentVariable("DB_PORT")};"
            + $"Username={Environment.GetEnvironmentVariable("DB_USER")};"
            + $"Password={Environment.GetEnvironmentVariable("DB_PASS")};"
            + $"Database={Environment.GetEnvironmentVariable("DB_NAME")};"
            + $"SSL Mode=none;";

        public DbSet<Job> Jobs { get; set; }

        public DbSet<FeatureStatus> FeatureStatuses { get; set; }

        public DbSet<Settings> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            JsonSerializerSettings settings = new()
            {
                Formatting = Formatting.None,
            };

            modelBuilder.Entity<Transaction>()
                .Property(s => s.Reason)
                .HasConversion(new EnumToStringConverter<Reason>());

            modelBuilder.Entity<Poll>()
                .Property(s => s.Type)
                .HasConversion(new EnumToStringConverter<PollType>());

            modelBuilder.Entity<Poll>()
                .Property(s => s.Status)
                .HasConversion(new EnumToStringConverter<PollStatus>());

            base.OnModelCreating(modelBuilder);
        }
    }
}