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
    public class SetLotterySubscribeCommand(IServiceScopeFactory scopeFactory, BotLog botLog)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly BotLog botLog = botLog;

        public async Task<Result> ExecuteAsync(Account account, bool subscribe)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                account.subscribeToLottery = subscribe;
                db.Accounts.Update(account);
                await db.SaveChangesAsync();
                Log.Debug($"Updated lottery subscription for {account.Username} | {account.Id} to {subscribe}");
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Edit, $"Updated lottery subscription for {account.Username} | {account.Id} to {subscribe}");
                return new Result(true, "Lottery subscription updated successfully.", account);
            }
            catch (Exception e)
            {
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Warning, $"Failed to update lottery subscription for {account.Username} | {account.Id} | Stack trace: {e.StackTrace}");
                Log.Error(e, $"Failed to update lottery subscription for {account.Id}.");
                return new Result(false, "Failed to update lottery subscription.", account);
            }
        }
    }
}
