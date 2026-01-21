using Fitz.Api.Controllers.Bank.GetTopBalances.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Bank.GetTopBalances.Persistence;

public class GetTopBalances(IDbContextFactory<BotContext> contextFactory, ILogger<GetTopBalances> logger) : IGetTopBalances
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<GetTopBalances> _logger = logger;

    public async Task<List<AccountEntity>> GetTopBalancesAsync(int limit, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting top balances. Limit: {Limit}", limit);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var accounts = await context.Accounts
            .OrderByDescending(a => a.Beer)
            .Take(limit)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Top balances retrieved. Count: {Count}", accounts.Count);

        return accounts;
    }
}
