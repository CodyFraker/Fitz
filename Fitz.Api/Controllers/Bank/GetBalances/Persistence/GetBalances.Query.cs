using Fitz.Api.Controllers.Bank.GetBalances.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Bank.GetBalances.Persistence;

public class GetBalances(IDbContextFactory<BotContext> contextFactory, ILogger<GetBalances> logger) : IGetBalances
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<GetBalances> _logger = logger;

    public async Task<(List<AccountEntity> Accounts, int TotalCount)> GetBalancesAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting balances. Skip: {Skip}, Take: {Take}", skip, take);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var totalCount = await context.Accounts.CountAsync(cancellationToken);
        var accounts = await context.Accounts
            .OrderByDescending(a => a.Beer)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Balances retrieved. Count: {Count}, TotalCount: {TotalCount}", accounts.Count, totalCount);

        return (accounts, totalCount);
    }
}
