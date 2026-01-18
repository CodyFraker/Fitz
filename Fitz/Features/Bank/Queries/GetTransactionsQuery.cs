using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using Transaction = Fitz.Database.Entities.Transaction;

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

        public (List<Transaction> Transactions, int TotalCount) Execute(ulong userId, int skip, int take)
        {
            using var scope = scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

            var query = db.Transactions.Where(t => t.Sender == userId || t.Recipient == userId);
            var totalCount = query.Count();

            var transactions = query
                .OrderByDescending(x => x.Timestamp)
                .Skip(skip)
                .Take(take)
                .ToList();

            return (transactions, totalCount);
        }
    }
}
