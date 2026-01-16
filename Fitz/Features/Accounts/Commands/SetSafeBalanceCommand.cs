using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Features.Accounts.Models;
using Fitz.Variables.Emojis;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts.Commands
{
    public class SetSafeBalanceCommand(IServiceScopeFactory scopeFactory, BotLog botLog)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly BotLog botLog = botLog;

        public async Task<Result> ExecuteAsync(Account account, int safeBalance)
        {
            if (account == null)
            {
                return new Result(false, "Account settings not found.", account);
            }

            try
            {
                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                account.safeBalance = safeBalance;
                db.Accounts.Update(account);
                await db.SaveChangesAsync();
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Edit, $"Updated safe balance for {account.Username} | {account.Id} to {safeBalance}");
                return new Result(true, "Safe balance updated successfully.", account);
            }
            catch (Exception e)
            {
                Log.Error(e, $"Failed to update safe balance for {account.Id}.");
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Warning, $"Failed to update safe balance for {account.Username} | {account.Id} | Stack trace: {e.StackTrace}");
                return new Result(false, "Failed to update safe balance.", account);
            }
        }
    }
}
