using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace Fitz.Features.Bank.Queries
{
    public class GetBalancesQuery(IServiceScopeFactory scopeFactory)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;

        public (List<Account> Accounts, int TotalCount) Execute(int skip = 0, int take = 10)
        {
            using var scope = scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

            var totalCount = db.Accounts.Count();
            var accounts = db.Accounts
                .OrderByDescending(a => a.Beer)
                .Skip(skip)
                .Take(take)
                .ToList();

            return (accounts, totalCount);
        }
    }
}
