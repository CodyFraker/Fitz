using DSharpPlus;
using Fitz.Database;
using Fitz.Core.Discord;
using Fitz.Core.Services;
using Fitz.Core.Services.Features;
using Fitz.Features.Bank;
using Fitz.Features.Favorability;
using Hangfire;
using Hangfire.MySql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Extensions.Logging;
using System;

namespace Fitz.Core
{
    public class ServiceRegistrant : IServiceRegistrant
    {
        public void ConfigureServices(IServiceCollection services)
        {
            ServerVersion? version = null;
            try
            {
                version = ServerVersion.AutoDetect(DatabaseConnection.ConnectionString);
            }
            catch
            {
                version = null;
            }

            if (version != null)
            {
                services.AddDbContext<BotContext>(
                    DbContextOptions => DbContextOptions
                    .UseMySql(DatabaseConnection.ConnectionString, version));
            }
            else
            {
                services.AddDbContext<BotContext>(
                    DbContextOptions => DbContextOptions
                    .UseInMemoryDatabase("TestDb"));
            }

            var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN");
            if (string.IsNullOrEmpty(botToken))
            {
                Log.Fatal("BOT_TOKEN environment variable is not set. The bot cannot start without a Discord token.");
                throw new InvalidOperationException("BOT_TOKEN environment variable is required but not set.");
            }

            try
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                services.AddSingleton(new DiscordClient(new DiscordConfiguration
                {
                    Intents = DiscordIntents.All,
                    LoggerFactory = new SerilogLoggerFactory(Log.Logger),
                    AlwaysCacheMembers = false,
                    AutoReconnect = true,
                    MessageCacheSize = 0,
                    Token = botToken,
                    TokenType = TokenType.Bot,
                }));
#pragma warning restore CA2000 // Dispose objects before losing scope
                Log.Information("DiscordClient registered successfully");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to register DiscordClient. The bot cannot start.");
                throw;
            }

            services.AddSingleton<BotLog>(sp =>
            {
                var client = sp.GetService<DiscordClient>();
                return new BotLog(client ?? throw new InvalidOperationException("DiscordClient is required for BotLog"));
            });

            services.AddSingleton<ActivityManager>(sp =>
            {
                var client = sp.GetService<DiscordClient>();
                return new ActivityManager(client ?? throw new InvalidOperationException("DiscordClient is required for ActivityManager"));
            });

            if (version != null)
            {
                try
                {
                    services.AddHangfire(config =>
                        config.UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings()
                        .UseStorage(
                            new MySqlStorage(DatabaseConnection.ConnectionString,
                            new MySqlStorageOptions
                            {
                                TransactionIsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                                QueuePollInterval = TimeSpan.FromSeconds(15),
                                JobExpirationCheckInterval = TimeSpan.FromHours(1),
                                CountersAggregateInterval = TimeSpan.FromMinutes(5),
                                PrepareSchemaIfNecessary = true,
                                DashboardJobListLimit = 50000,
                                TransactionTimeout = TimeSpan.FromMinutes(1),
                                TablesPrefix = "hangfire"
                            })))
                    .AddHangfireServer();
                }
                catch { }
            }

            services.AddSingleton<FeatureManager>()
                .AddSingleton<BankService>()
                .AddTransient<FavorabilityService>();
        }
    }
}