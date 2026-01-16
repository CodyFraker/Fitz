using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Features.Lottery.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Lottery.Commands
{
    public class AddToPoolCommand(IServiceScopeFactory scopeFactory, BotLog botLog)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly BotLog botLog = botLog;

        public async Task<Result> ExecuteAsync(int amount)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                var getCurrentLotteryQuery = new Queries.GetCurrentLotteryQuery(scopeFactory);
                var lottery = getCurrentLotteryQuery.Execute();

                if (lottery == null)
                {
                    return new Result(false, "No active lottery found.", null);
                }

                lottery.Pool += amount;
                db.Update(lottery);
                await db.SaveChangesAsync();

                Log.Debug($"Added {amount} to lottery pool. New pool: {lottery.Pool}");
                this.botLog.Information(LogConsoleSettings.LotteryLog, Variables.Emojis.LotteryEmojis.Lottery, $"Added {amount} to lottery pool. New pool: {lottery.Pool}");

                return new Result(true, $"Added {amount} to lottery pool.", lottery);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to add to lottery pool.");
                return new Result(false, "Failed to add to lottery pool.", null);
            }
        }
    }
}
