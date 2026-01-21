using Fitz.Api.Controllers.Bank.GetUserTransactions.Domain;
using Fitz.Database;
using Microsoft.EntityFrameworkCore;
using Transaction = Fitz.Database.Entities.Transaction;

namespace Fitz.Api.Controllers.Bank.GetUserTransactions.Persistence;

public class GetUserTransactions(IDbContextFactory<BotContext> contextFactory, ILogger<GetUserTransactions> logger) : IGetUserTransactions
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<GetUserTransactions> _logger = logger;

    public async Task<(List<Transaction> Transactions, int TotalCount)> GetUserTransactionsAsync(ulong userId, int skip, int take, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting user transactions. UserId: {UserId}, Skip: {Skip}, Take: {Take}", userId, skip, take);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Transactions.Where(t => t.Sender == userId || t.Recipient == userId);
        var totalCount = await query.CountAsync(cancellationToken);

        var transactions = await query
            .OrderByDescending(t => t.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("User transactions retrieved. Count: {Count}, TotalCount: {TotalCount}", transactions.Count, totalCount);

        return (transactions, totalCount);
    }
}
