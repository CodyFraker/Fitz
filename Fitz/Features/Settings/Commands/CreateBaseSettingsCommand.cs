using Fitz.Core.Models;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Settings.Commands
{
    public class CreateBaseSettingsCommand(IServiceScopeFactory scopeFactory)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;

        public async Task<Result> ExecuteAsync()
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                var settings = db.Settings.FirstOrDefault();

                if (settings != null)
                {
                    return new Result(false, "Settings already exist.", null);
                }

                settings = new Database.Entities.Settings
                {
                    LotteryDuration = 7,
                    BaseLotteryPool = 36,
                    LotteryPoolRollover = true,
                    TicketCost = 1,
                    MaxTickets = 128,
                    AccountCreationBonusAmount = 128,
                    BaseHappyHourAmount = 6,
                    RenameBaseCost = 6,
                    PollApprovedBonus = 24,
                    PollSubmittedPenalty = 36,
                    PollDeclinedPenalty = 0,
                    PollVote = 12,
                    PollCreatorTip = 6,
                    MaxPendingPolls = 10,
                    FavorabilityBeerRatioThreshold = 2.0m,
                    FavorabilityLowThreshold = 10,
                    FavorabilityBaseDropPercent = 1.0m,
                    FavorabilityDropMultiplier = 1.5m
                };

                db.Settings.Add(settings);
                await db.SaveChangesAsync();
                return new Result(true, "Base settings created.", settings);
            }
            catch (Exception ex)
            {
                return new Result(false, $"Failed creating base settings. Exception message: {ex.Message}", null);
            }
        }
    }
}
