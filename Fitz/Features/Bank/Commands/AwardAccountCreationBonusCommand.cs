using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank.Models;
using Fitz.Features.Settings.Queries;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Transaction = Fitz.Features.Bank.Models.Transaction;

namespace Fitz.Features.Bank.Commands
{
    public class AwardAccountCreationBonusCommand(IServiceScopeFactory scopeFactory, BotLog botLog)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly BotLog botLog = botLog;

        public async Task<Result> ExecuteAsync(Account account)
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
                
                return new Result(true, $"Awarded account creation bonus to {account.Username}.", account);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }
    }
}
