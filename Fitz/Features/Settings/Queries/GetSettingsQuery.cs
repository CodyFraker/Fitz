using Fitz.Core.Contexts;
using Fitz.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Fitz.Features.Settings.Queries
{
    public class GetSettingsQuery(IServiceScopeFactory scopeFactory)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;

        public Core.Models.Settings Execute()
        {
            using var scope = scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

            var settings = db.Settings.FirstOrDefault();

            if (settings == null)
            {
                var createCommand = new Commands.CreateBaseSettingsCommand(scopeFactory);
                createCommand.ExecuteAsync().Wait();
                settings = db.Settings.FirstOrDefault();
            }

            return settings;
        }
    }
}
