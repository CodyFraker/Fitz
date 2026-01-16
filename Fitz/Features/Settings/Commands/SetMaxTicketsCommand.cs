using Fitz.Core.Contexts;
using Fitz.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Settings.Commands
{
    public class SetMaxTicketsCommand(IServiceScopeFactory scopeFactory)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;

        public async Task<Result> ExecuteAsync(int maxTickets)
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

                if (maxTickets < 0)
                {
                    return new Result(false, "The maximum tickets must be a positive number.", null);
                }

                settings.MaxTickets = maxTickets;
                db.Update(settings);
                await db.SaveChangesAsync();

                return new Result(true, $"Set max tickets to {maxTickets}.", settings);
            }
            catch (Exception ex)
            {
                return new Result(false, $"Failed setting max tickets. Exception message: {ex.Message}", null);
            }
        }
    }
}
