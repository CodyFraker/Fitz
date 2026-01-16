using Fitz.Core.Contexts;
using Fitz.Features.Bank.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using Transaction = Fitz.Features.Bank.Models.Transaction;

namespace Fitz.Features.Bank.Queries
{
    public class GetTransactionsQuery(IServiceScopeFactory scopeFactory)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;

        public List<Transaction> Execute(int take)
        {
            using var scope = scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return db.Transactions.OrderByDescending(t => t.Timestamp).Take(take).ToList();
        }

        public List<Transaction> Execute(ulong userId)
        {
            using var scope = scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return db.Transactions.Where(t => t.Sender == userId || t.Recipient == userId).OrderByDescending(x => x.Timestamp).ToList();
        }
    }
}
