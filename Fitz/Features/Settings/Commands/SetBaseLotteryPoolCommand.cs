using Fitz.Database;
using Fitz.Core.Models;
using Fitz.Database.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Settings.Commands
{
    public class SetBaseLotteryPoolCommand(IServiceScopeFactory scopeFactory)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;

        public async Task<Result> ExecuteAsync(int pool)
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

                if (pool < 0)
                {
                    return new Result(false, "The base lottery pool must be a positive number.", null);
                }

                settings.BaseLotteryPool = pool;
                db.Update(settings);
                await db.SaveChangesAsync();

                return new Result(true, $"Set base lottery pool to {pool}.", settings);
            }
            catch (Exception ex)
            {
                return new Result(false, $"Failed setting base lottery pool. Exception message: {ex.Message}", null);
            }
        }
    }
}
