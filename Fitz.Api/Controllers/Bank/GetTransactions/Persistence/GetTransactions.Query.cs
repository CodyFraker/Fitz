using Fitz.Api.Controllers.Bank.GetTransactions.Domain;
using Fitz.Database;
using Microsoft.EntityFrameworkCore;
using Transaction = Fitz.Database.Entities.Transaction;

namespace Fitz.Api.Controllers.Bank.GetTransactions.Persistence;

public class GetTransactions(IDbContextFactory<BotContext> contextFactory, ILogger<GetTransactions> logger) : IGetTransactions
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<GetTransactions> _logger = logger;

    public async Task<List<Transaction>> GetTransactionsAsync(int take, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting transactions. Take: {Take}", take);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var transactions = await context.Transactions
            .OrderByDescending(t => t.Timestamp)
            .Take(take)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Transactions retrieved. Count: {Count}", transactions.Count);

        return transactions;
    }
}
