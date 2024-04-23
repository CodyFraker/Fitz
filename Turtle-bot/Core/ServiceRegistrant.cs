using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Services;
using Fitz.Core.Services.Jobs;
using Fitz.Core.Services.Features;
using System.Transactions;
using Fitz.Features.Accounts;
using Fitz.Features.Bank;

namespace Fitz.Core
{
    public class ServiceRegistrant : IServiceRegistrant
    {
        public void ConfigureServices(IServiceCollection services)
        {
            ServerVersion version = ServerVersion.AutoDetect(BotContext.ConnectionString);

            services.AddSingleton<BotLog>()
                .AddDbContextPool<BotContext>(options => options.UseMySql(BotContext.ConnectionString, version))
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
                .AddSingleton<AccountService>()
                .AddSingleton<BankService>()
                .AddSingleton<JobManager>();
        }
    }
}