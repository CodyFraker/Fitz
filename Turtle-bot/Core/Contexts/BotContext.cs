namespace Fitz.Core.Contexts
{
    using Bot.Features.FAQ.Models;
    using Fitz.Core.Services.Features;
    using Fitz.Core.Services.Jobs;
    using Fitz.Features.Accounts.Models;
    using Fitz.Features.Bank.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using Newtonsoft.Json;
    using System;
    using System.Text.RegularExpressions;

    public partial class BotContext : DbContext
    {
        public BotContext(DbContextOptions<BotContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Pulls variables stored in the .env file.
        /// </summary>
        public static string ConnectionString =>
            $"Host={Environment.GetEnvironmentVariable("DB_HOST")};"
            + $"Port={Environment.GetEnvironmentVariable("DB_PORT")};"
            + $"Username={Environment.GetEnvironmentVariable("DB_USER")};"
            + $"Password={Environment.GetEnvironmentVariable("DB_PASS")};"
            + $"Database={Environment.GetEnvironmentVariable("DB_NAME")};"
            + $"SSL Mode=none";

        public DbSet<Job> Jobs { get; set; }

        public DbSet<FeatureStatus> FeatureStatuses { get; set; }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.None,
            };

            modelBuilder.Entity<Faq>()
                .Property(s => s.Regex)
                .HasConversion(s => s.ToString(), s => new Regex(s, RegexOptions.Compiled | RegexOptions.IgnoreCase));

            modelBuilder.Entity<Transaction>()
                .Property(s => s.Reason)
                .HasConversion(new EnumToStringConverter<Reason>());

            base.OnModelCreating(modelBuilder);
        }
    }
}