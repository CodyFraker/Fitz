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
    public class SetFavorabilityCommand(IServiceScopeFactory scopeFactory, BotLog botLog)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly BotLog botLog = botLog;

        public async Task<Result> ExecuteAsync(AccountEntity account, int newFavorability)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                if (account.Favorability >= 100)
                {
                    return new Result(false, "User already has max favorability.", account);
                }

                account.Favorability = newFavorability;
                db.Accounts.Update(account);
                await db.SaveChangesAsync();
                Log.Debug($"{newFavorability} Favorability added to {account.Id} successfully.");
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Edit, $"{newFavorability} Favorability added to {account.Username} | {account.Id} successfully.");
                return new Result(true, $"Favorability added to {account.Id} successfully.", account);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to update favorability for {account.Id}.");
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Warning, $"Failed to update favorability for {account.Username} | {account.Id} | Stack trace: {ex.StackTrace}");
                return new Result(false, ex.Message, null);
            }
        }
    }
}
