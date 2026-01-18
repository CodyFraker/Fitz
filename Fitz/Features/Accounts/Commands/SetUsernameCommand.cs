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
    public class SetUsernameCommand(IServiceScopeFactory scopeFactory, BotLog botLog)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly BotLog botLog = botLog;

        public async Task<Result> ExecuteAsync(Account account, string username)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                if (account == null)
                {
                    return new Result(false, "Account not found.", account);
                }

                account.Username = username;
                db.Accounts.Update(account);
                await db.SaveChangesAsync();
                Log.Debug($"Updated username for {account.Id} to {username}");
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Edit, $"Updated username for {account.Username} | {account.Id} to {username}");
                return new Result(true, "Username updated successfully.", account);
            }
            catch (Exception e)
            {
                Log.Error(e, $"Failed to update username for {account.Id}.");
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Warning, $"Failed to update username for {account.Username} | {account.Id} | Stack trace: {e.StackTrace}");
                return new Result(false, "Failed to update username.", account);
            }
        }
    }
}
