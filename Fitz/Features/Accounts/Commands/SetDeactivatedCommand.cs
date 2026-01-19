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
    public class SetDeactivatedCommand(IServiceScopeFactory scopeFactory, BotLog botLog)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly BotLog botLog = botLog;

        public async Task<Result> ExecuteAsync(AccountEntity account, bool deactivated)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                if (account == null)
                {
                    return new Result(false, "Account not found.", account);
                }

                account.Deactivated = deactivated;
                db.Accounts.Update(account);
                await db.SaveChangesAsync();
                Log.Debug($"Updated deactivated status for {account.Id} to {deactivated}");
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Edit, $"Updated deactivated status for {account.Username} | {account.Id} to {deactivated}");
                return new Result(true, "Deactivated status updated successfully.", account);
            }
            catch (Exception e)
            {
                Log.Error(e, $"Failed to update deactivated status for {account.Id}.");
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Warning, $"Failed to update deactivated status for {account.Username} | {account.Id} | Stack trace: {e.StackTrace}");
                return new Result(false, "Failed to update deactivated status.", account);
            }
        }
    }
}
