using DSharpPlus.Entities;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Fitz.Features.Accounts.Queries
{
    public class FindAccountQuery(IServiceScopeFactory scopeFactory)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;

        public AccountEntity Execute(ulong id)
        {
            using var scope = scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            return db.Accounts.Where(x => x.Id == id).FirstOrDefault();
        }

        public AccountEntity Execute(DiscordUser user)
        {
            return Execute(user.Id);
        }
    }
}
