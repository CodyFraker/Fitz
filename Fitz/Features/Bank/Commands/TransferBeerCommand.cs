using Fitz.Database;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Database.Entities;
using Fitz.Features.Accounts;
using Fitz.Database.Entities;
using Fitz.Database.Entities;
using Fitz.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Bank.Commands
{
    public class TransferBeerCommand(IServiceScopeFactory scopeFactory, AccountService accountService, BotLog botLog, FitzMetrics? fitzMetrics = null)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly AccountService accountService = accountService;
        private readonly BotLog botLog = botLog;
        private readonly FitzMetrics? fitzMetrics = fitzMetrics;

        public async Task<Result> ExecuteAsync(ulong sender, ulong recipient, int amount)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                AccountEntity senderAccount = accountService.FindAccount(sender);
                if (senderAccount == null)
                {
                    Log.Error($"Sender account not found. {sender}");
                    return new Result(false, $"Sender account not found for user {sender}.", null);
                }

                AccountEntity recipientAccount = accountService.FindAccount(recipient);
                if (recipientAccount == null)
                {
                    Log.Error($"Recipient account not found. {recipient}");
                    return new Result(false, $"Recipient account not found for user {recipient}.", null);
                }

                if (senderAccount.Beer < amount)
                {
                    Log.Error($"Sender does not have enough beer to give. {sender}");
                    return new Result(false, $"Sender does not have enough beer to transfer.", null);
                }

                senderAccount.Beer -= amount;
                db.Update(senderAccount);
                await db.SaveChangesAsync();
                
                recipientAccount.LifetimeBeer += amount;
                recipientAccount.Beer += amount;
                db.Update(recipientAccount);
                await db.SaveChangesAsync();

                var logTransactionCommand = new LogTransactionCommand(scopeFactory, botLog);
                await logTransactionCommand.ExecuteAsync(senderAccount, recipientAccount, amount, Reason.Donated);
                
                fitzMetrics?.RecordBeerTransfer(amount, Reason.Donated.ToString());
                fitzMetrics?.RecordTransaction("transfer");
                
                return new Result(true, $"Transferred {amount} beer from {senderAccount.Username} to {recipientAccount.Username}.", senderAccount);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }
    }
}
