using Fitz.Database;
using Fitz.Core.Models;
using Fitz.Database.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Settings.Commands
{
    public class SetLotteryDurationCommand(IServiceScopeFactory scopeFactory)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;

        public async Task<Result> ExecuteAsync(int days)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                var settings = db.Settings.FirstOrDefault();

                if (settings == null)
                {
                    var createCommand = new CreateBaseSettingsCommand(scopeFactory);
                    await createCommand.ExecuteAsync();
                    settings = db.Settings.FirstOrDefault();
                }

                if (days > 365)
                {
                    return new Result(false, "The maximum lottery duration is 365 days.", null);
                }
                if (days < 0)
                {
                    return new Result(false, "The lottery duration must be longer than a single day.", null);
                }

                settings.LotteryDuration = days;
                db.Update(settings);
                await db.SaveChangesAsync();

                return new Result(true, $"Set lottery duration to {days} day(s).", settings);
            }
            catch (Exception ex)
            {
                return new Result(false, $"Failed setting lottery duration. Exception message: {ex.Message}", null);
            }
        }
    }
}
