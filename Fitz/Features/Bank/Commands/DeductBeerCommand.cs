using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank.Models;
using Fitz.Metrics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Bank.Commands
{
    public class DeductBeerCommand(IServiceScopeFactory scopeFactory, AccountService accountService, BotLog botLog, FitzMetrics? fitzMetrics = null)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly AccountService accountService = accountService;
        private readonly BotLog botLog = botLog;
        private readonly FitzMetrics? fitzMetrics = fitzMetrics;

        public async Task<Result> ExecuteAsync(ulong userId, int amount, Reason reason)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                Account account = accountService.FindAccount(userId);
                if (account == null)
                {
                    return new Result(false, $"{userId} did not have an account.", null);
                }
                if (account.Beer < amount)
                {
                    return new Result(false, $"{userId} did not have enough beer to deduct.", null);
                }

                account.Beer -= amount;
                db.Update(account);
                await db.SaveChangesAsync();
                
                var logTransactionCommand = new LogTransactionCommand(scopeFactory, botLog);
                await logTransactionCommand.ExecuteAsync(account, account, amount, reason);

                fitzMetrics?.RecordBeerDeduction(amount, reason.ToString());
                fitzMetrics?.RecordTransaction("deduction");

                return new Result(true, $"Deducted {amount} beer from {account.Username}.", account);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }
    }
}
