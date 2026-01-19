using Fitz.Database;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Database.Entities;
using Fitz.Database.Entities;
using Fitz.Variables.Emojis;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts.Commands
{
    public class SetTicketAmountCommand(IServiceScopeFactory scopeFactory, BotLog botLog)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly BotLog botLog = botLog;

        public async Task<Result> ExecuteAsync(AccountEntity account, int amount)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                account.SubscribeTickets = amount;
                db.Update(account);
                await db.SaveChangesAsync();
                Log.Debug($"Updated ticket amount for {account.Username} | {account.Id} to {amount}");
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Edit, $"Updated ticket amount for {account.Username} | {account.Id} to {amount}");
                return new Result(true, "Ticket amount updated successfully.", account);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to update ticket amount for {account.Id}.");
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Warning, $"Failed to update ticket amount for {account.Username} | {account.Id} | Stack trace: {ex.StackTrace}");
                return new Result(false, ex.Message, null);
            }
        }
    }
}
