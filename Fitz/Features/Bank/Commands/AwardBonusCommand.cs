using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Bank.Commands
{
    public class AwardBonusCommand(IServiceScopeFactory scopeFactory, AccountService accountService, BotLog botLog)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly AccountService accountService = accountService;
        private readonly BotLog botLog = botLog;

        public async Task<Result> ExecuteAsync(ulong userId, int amount)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                Account account = accountService.FindAccount(userId);
                if (account == null)
                {
                    Log.Error($"Account not found. {userId}");
                    return new Result(false, $"Account not found for user {userId}.", null);
                }

                account.Beer += amount;
                account.LifetimeBeer += amount;
                db.Update(account);
                await db.SaveChangesAsync();
                
                var logTransactionCommand = new LogTransactionCommand(scopeFactory, botLog);
                await logTransactionCommand.ExecuteAsync(account, account, amount, Reason.Bonus);
                
                return new Result(true, $"Awarded {amount} beer to {account.Username}.", account);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }
    }
}
