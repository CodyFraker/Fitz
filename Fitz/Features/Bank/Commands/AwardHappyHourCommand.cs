using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank.Models;
using Fitz.Features.Settings;
using Fitz.Metrics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Bank.Commands
{
    public class AwardHappyHourCommand(IServiceScopeFactory scopeFactory, AccountService accountService, SettingsService settingsService, BotLog botLog, FitzMetrics? fitzMetrics = null)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly AccountService accountService = accountService;
        private readonly SettingsService settingsService = settingsService;
        private readonly BotLog botLog = botLog;
        private readonly FitzMetrics? fitzMetrics = fitzMetrics;

        public async Task<Result> ExecuteAsync(ulong userId)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();
                var settings = settingsService.GetSettings();

                Account account = accountService.FindAccount(userId);
                if (account == null)
                {
                    return new Result(false, $"{userId} did not have an account.", null);
                }

                account.Beer += settings.BaseHappyHourAmount;
                account.LifetimeBeer += settings.BaseHappyHourAmount;
                db.Update(account);
                await db.SaveChangesAsync();
                
                var logTransactionCommand = new LogTransactionCommand(scopeFactory, botLog);
                await logTransactionCommand.ExecuteAsync(account, account, settings.BaseHappyHourAmount, Reason.HappyHour);

                fitzMetrics?.RecordBeerAward(settings.BaseHappyHourAmount, Reason.HappyHour.ToString());
                fitzMetrics?.RecordTransaction("happy_hour");

                return new Result(true, $"Awarded happy hour bonus to {account.Username}.", account);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }
    }
}
