using DSharpPlus;
using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Services;
using Fitz.Core.Services.Features;
using Fitz.Core.Services.Jobs;
using Fitz.Core.Services.Settings;
using Fitz.Features.Bank;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using Hangfire;
using Hangfire.MySql;

namespace Fitz.Core
{
    public class ServiceRegistrant : IServiceRegistrant
    {
        public void ConfigureServices(IServiceCollection services)
        {
            ServerVersion version = ServerVersion.AutoDetect(BotContext.ConnectionString);

            services.AddDbContext<BotContext>(
                DbContextOptions => DbContextOptions
                .UseMySql(BotContext.ConnectionString, version))
                .AddSingleton<BotLog>()
                .AddSingleton<ActivityManager>()
                .AddHangfire(config =>
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
                .AddHangfireServer()
#pragma warning disable CA2000 // Dispose objects before losing scope
                .AddSingleton(new DiscordClient(new DiscordConfiguration
                {
                    Intents = DiscordIntents.All,
                    LoggerFactory = new SerilogLoggerFactory(Log.Logger),
                    AlwaysCacheMembers = false,
                    AutoReconnect = true,
                    MessageCacheSize = 0,
                    Token = Environment.GetEnvironmentVariable("BOT_TOKEN"),
                    TokenType = TokenType.Bot,
                }))
#pragma warning restore CA2000 // Dispose objects before losing scope
                .AddSingleton<FeatureManager>()
                .AddSingleton<BankService>()
                .AddSingleton<SettingsService>();
        }
    }
}