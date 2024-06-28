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
                //.AddDbContextPool<BotContext>(options => options.UseMySql(BotContext.ConnectionString, version))
                .AddSingleton<ActivityManager>()

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
                .AddSingleton<SettingsService>()
                .AddSingleton<JobManager>();
        }
    }
}