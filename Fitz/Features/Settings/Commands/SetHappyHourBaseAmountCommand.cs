using Fitz.Core.Contexts;
using Fitz.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Settings.Commands
{
    public class SetHappyHourBaseAmountCommand(IServiceScopeFactory scopeFactory)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;

        public async Task<Result> ExecuteAsync(int amount)
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

                if (amount < 0)
                {
                    return new Result(false, "The happy hour base amount must be a positive number.", null);
                }

                settings.BaseHappyHourAmount = amount;
                db.Update(settings);
                await db.SaveChangesAsync();

                return new Result(true, $"Set happy hour base amount to {amount}.", settings);
            }
            catch (Exception ex)
            {
                return new Result(false, $"Failed setting happy hour base amount. Exception message: {ex.Message}", null);
            }
        }
    }
}
