using Fitz.Database.Entities;
using Fitz.Features.Accounts.Queries;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Fitz.Features.Bank.Queries
{
    public class GetBalanceQuery(IServiceScopeFactory scopeFactory)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;

        public int Execute(ulong userId)
        {
            var findAccountQuery = new Accounts.Queries.FindAccountQuery(scopeFactory);
            var account = findAccountQuery.Execute(userId);
            if (account == null)
            {
                Log.Error($"Account not found. {userId}");
                return 0;
            }

            return account.Beer;
        }
    }
}
