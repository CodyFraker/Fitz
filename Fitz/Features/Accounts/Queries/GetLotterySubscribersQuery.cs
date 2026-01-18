using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace Fitz.Features.Accounts.Queries
{
    public class GetLotterySubscribersQuery(IServiceScopeFactory scopeFactory)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;

        public List<Account> Execute()
        {
            using var scope = scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            return db.Accounts.Where(x => x.subscribeToLottery == true && x.Deactivated == false).ToList();
        }
    }
}
