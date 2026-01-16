using Fitz.Core.Contexts;
using Fitz.Features.Accounts.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace Fitz.Features.Bank.Queries
{
    public class GetTopBeerBalancesQuery(IServiceScopeFactory scopeFactory)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;

        public List<Account> Execute(int limit = 10)
        {
            using var scope = scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return db.Accounts.OrderByDescending(a => a.Beer).Take(limit).ToList();
        }
    }
}
