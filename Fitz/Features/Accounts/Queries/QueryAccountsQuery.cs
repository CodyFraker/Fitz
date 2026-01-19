using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace Fitz.Features.Accounts.Queries
{
    public class QueryAccountsQuery(IServiceScopeFactory scopeFactory)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;

        public List<AccountEntity> Execute()
        {
            var dbAccounts = new List<AccountEntity>();

            using var scope = scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            try
            {
                var dbQuery = db.Accounts;
                foreach (var account in dbQuery)
                {
                    dbAccounts.Add(account);
                }

                return dbAccounts;
            }
            catch (System.Exception e)
            {
                Log.Error(e, "Failed to query all accounts!");
            }

            return dbAccounts;
        }
    }
}
