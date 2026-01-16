using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Accounts.Queries;
using Fitz.Features.Bank.Models;
using Fitz.Variables;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Transaction = Fitz.Features.Bank.Models.Transaction;

namespace Fitz.Features.Bank.Commands
{
    public class TransferToFitzCommand(IServiceScopeFactory scopeFactory, BotLog botLog)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly BotLog botLog = botLog;

        public async Task<Result> ExecuteAsync(ulong userId, int amount, Reason reason)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                var findAccountQuery = new Accounts.Queries.FindAccountQuery(scopeFactory);
                var account = findAccountQuery.Execute(userId);
                if (account == null)
                {
                    return new Result(false, $"{userId} did not have an account.", null);
                }

                if (account.Beer < amount)
                {
                    return new Result(false, $"{userId} did not have enough beer to transfer.", null);
                }
                var fitz = findAccountQuery.Execute(Users.Fitz);

                account.Beer -= amount;
                fitz.Beer += amount;
                fitz.LifetimeBeer += amount;

                db.Update(account);
                await db.SaveChangesAsync();
                db.Update(fitz);
                await db.SaveChangesAsync();
                
                var logTransactionCommand = new LogTransactionCommand(scopeFactory, botLog);
                await logTransactionCommand.ExecuteAsync(account, fitz, amount, reason);
                
                return new Result(true, $"Transferred {amount} beer to Fitz.", account);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }
    }
}
