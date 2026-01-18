using Fitz.Database;
using Fitz.Core.Models;
using Fitz.Database.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Settings.Commands
{
    public class SetLotteryPoolRolloverCommand(IServiceScopeFactory scopeFactory)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;

        public async Task<Result> ExecuteAsync(bool rollover)
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

                settings.LotteryPoolRollover = rollover;
                db.Update(settings);
                await db.SaveChangesAsync();

                return new Result(true, $"Set lottery pool rollover to {rollover}.", settings);
            }
            catch (Exception ex)
            {
                return new Result(false, $"Failed setting lottery pool rollover. Exception message: {ex.Message}", null);
            }
        }
    }
}
