using Fitz.Database;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Database.Entities;
using Fitz.Database.Entities;
using Fitz.Database.Entities;
using Fitz.Features.Settings.Queries;
using Fitz.Metrics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Transaction = Fitz.Database.Entities.Transaction;

namespace Fitz.Features.Bank.Commands
{
    public class AwardAccountCreationBonusCommand(IServiceScopeFactory scopeFactory, BotLog botLog, FitzMetrics? fitzMetrics = null)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly BotLog botLog = botLog;
        private readonly FitzMetrics? fitzMetrics = fitzMetrics;

        public async Task<Result> ExecuteAsync(AccountEntity account)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();
                var getSettingsQuery = new Settings.Queries.GetSettingsQuery(scopeFactory);
                var settings = getSettingsQuery.Execute();

                account.Beer += settings.AccountCreationBonusAmount;
                account.LifetimeBeer += settings.AccountCreationBonusAmount;

                db.Update(account);
                await db.SaveChangesAsync();
                
                var logTransactionCommand = new LogTransactionCommand(scopeFactory, botLog);
                await logTransactionCommand.ExecuteAsync(account, account, settings.AccountCreationBonusAmount, Reason.AccountCreationBonus);
                
                fitzMetrics?.RecordBeerAward(settings.AccountCreationBonusAmount, Reason.AccountCreationBonus.ToString());
                fitzMetrics?.RecordTransaction("account_creation_bonus");
                
                return new Result(true, $"Awarded account creation bonus to {account.Username}.", account);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }
    }
}
