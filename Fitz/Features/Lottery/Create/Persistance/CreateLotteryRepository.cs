using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Variables.Emojis;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Lottery.Create.Persistance
{
    public class CreateLotteryRepository
    {
        private readonly BotContext _dbContext;
        private readonly BotLog _botLog;

        public CreateLotteryRepository(BotContext dbContext, BotLog botLog)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _botLog = botLog ?? throw new ArgumentNullException(nameof(botLog));
        }

        public async Task<bool> PersistLottery(Models.Lottery lottery)
        {
            try
            {
                // Check if there's already a current lottery
                var currentLottery = await _dbContext.Set<Models.Lottery>()
                    .Where(l => l.CurrentLottery)
                    .FirstOrDefaultAsync();

                if (currentLottery != null)
                {
                    // Set the previous lottery to not be current
                    currentLottery.CurrentLottery = false;
                    _dbContext.Update(currentLottery);
                }

                // Set the new lottery as current
                lottery.CurrentLottery = true;

                // Add the new lottery
                await _dbContext.AddAsync(lottery);
                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _botLog.Information(LogConsoleSettings.LotteryLog, LotteryEmojis.Lottery,
                    $"Failed to persist lottery: {ex.Message}");
                return false;
            }
        }

        public async Task<Models.Lottery> GetCurrentLottery()
        {
            try
            {
                var currentLottery = await _dbContext.Set<Models.Lottery>()
                    .Where(l => l.CurrentLottery)
                    .FirstOrDefaultAsync();

                return currentLottery;
            }
            catch (Exception ex)
            {
                _botLog.Information(LogConsoleSettings.LotteryLog, LotteryEmojis.Lottery,
                    $"Failed to get current lottery: {ex.Message}");
                return null;
            }
        }
    }
}
