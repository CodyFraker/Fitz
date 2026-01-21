using Fitz.Api.Controllers.Users.GetUsers.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Users.GetUsers.Persistence;

public class GetUsers(IDbContextFactory<BotContext> contextFactory, ILogger<GetUsers> logger) : IGetUsers
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<GetUsers> _logger = logger;

    public async Task<(List<AccountEntity> Accounts, int TotalCount)> GetUsersAsync(string? query, int skip, int take, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting users. Query: {Query}, Skip: {Skip}, Take: {Take}", query, skip, take);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var accountsQuery = context.Accounts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var queryLower = query.ToLower();
            accountsQuery = accountsQuery.Where(a => a.Username != null && a.Username.ToLower().Contains(queryLower));
        }

        var totalCount = await accountsQuery.CountAsync(cancellationToken);

        var accounts = await accountsQuery
            .OrderBy(a => a.Username ?? string.Empty)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Users retrieved. TotalCount: {TotalCount}, Returned: {Returned}", totalCount, accounts.Count);

        return (accounts, totalCount);
    }
}
