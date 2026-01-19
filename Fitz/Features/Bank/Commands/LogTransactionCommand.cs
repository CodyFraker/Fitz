using Fitz.Database;
using Fitz.Core.Discord;
using Fitz.Database.Entities;
using Fitz.Database.Entities;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;
using Transaction = Fitz.Database.Entities.Transaction;

namespace Fitz.Features.Bank.Commands
{
    public class LogTransactionCommand(IServiceScopeFactory scopeFactory, BotLog botLog)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly BotLog botLog = botLog;

        public async Task ExecuteAsync(AccountEntity sender, AccountEntity recipient, int amount, Reason reason)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                var transaction = new Transaction
                {
                    Sender = sender.Id,
                    Recipient = recipient.Id,
                    Amount = amount,
                    Reason = reason,
                    Timestamp = DateTime.Now
                };
                db.Add(transaction);
                await db.SaveChangesAsync();
                Log.Debug($"Transaction logged: Sender: {sender.Username} | Recipient: {recipient.Username} | Amount: {amount}, Reason: {reason}");
                this.botLog.Information(LogConsoleSettings.Transactions, Variables.Emojis.BankEmojis.Transaction, $"Transaction logged: Sender: {sender.Username} | Recipient: {recipient.Username} | Amount: {amount}, Reason: {reason}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to log transaction.");
            }
        }
    }
}
