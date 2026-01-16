using DSharpPlus;
using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Services;
using Fitz.Core.Services.Features;
using Fitz.Features.Bank;
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
                version = ServerVersion.AutoDetect(BotContext.ConnectionString);
            }
            catch
            {
                version = null;
            }

            if (version != null)
            {
                services.AddDbContext<BotContext>(
                    DbContextOptions => DbContextOptions
                    .UseMySql(BotContext.ConnectionString, version));
            }
            else
            {
                services.AddDbContext<BotContext>(
                    DbContextOptions => DbContextOptions
                    .UseInMemoryDatabase("TestDb"));
            }

            services.AddSingleton<BotLog>()
                .AddSingleton<ActivityManager>();

            if (version != null)
            {
                try
                {
                    services.AddHangfire(config =>
                        config.UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings()
                        .UseStorage(
                            new MySqlStorage(BotContext.ConnectionString,
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
                    Token = Environment.GetEnvironmentVariable("BOT_TOKEN"),
                    TokenType = TokenType.Bot,
                }));
#pragma warning restore CA2000 // Dispose objects before losing scope
            }
            catch { }

            services.AddSingleton<FeatureManager>()
                .AddSingleton<BankService>();
        }
    }
}