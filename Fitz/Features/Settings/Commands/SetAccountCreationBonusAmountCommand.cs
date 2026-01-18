using Fitz.Database;
using Fitz.Core.Models;
using Fitz.Database.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Settings.Commands
{
    public class SetAccountCreationBonusAmountCommand(IServiceScopeFactory scopeFactory)
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
                    return new Result(false, "The account creation bonus amount must be a positive number.", null);
                }

                settings.AccountCreationBonusAmount = amount;
                db.Update(settings);
                await db.SaveChangesAsync();

                return new Result(true, $"Set account creation bonus amount to {amount}.", settings);
            }
            catch (Exception ex)
            {
                return new Result(false, $"Failed setting account creation bonus amount. Exception message: {ex.Message}", null);
            }
        }
    }
}
