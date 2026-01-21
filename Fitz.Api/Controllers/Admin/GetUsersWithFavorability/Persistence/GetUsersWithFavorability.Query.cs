using Fitz.Api.Controllers.Admin.GetUsersWithFavorability.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Admin.GetUsersWithFavorability.Persistence;

public class GetUsersWithFavorability(IDbContextFactory<BotContext> contextFactory, ILogger<GetUsersWithFavorability> logger) : IGetUsersWithFavorability
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<GetUsersWithFavorability> _logger = logger;

    public async Task<(List<AccountEntity> Accounts, int TotalCount)> GetUsersAsync(string? query, int skip, int take, string? sortBy, string? sortOrder, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting users with favorability. Query: {Query}, Skip: {Skip}, Take: {Take}, SortBy: {SortBy}, SortOrder: {SortOrder}", 
            query, skip, take, sortBy, sortOrder);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var accountsQuery = context.Accounts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            if (ulong.TryParse(query, out ulong userId))
            {
                accountsQuery = accountsQuery.Where(a => a.Id == userId);
            }
            else
            {
                accountsQuery = accountsQuery.Where(a => a.Username != null && a.Username.Contains(query));
            }
        }

        var totalCount = await accountsQuery.CountAsync(cancellationToken);

        IQueryable<AccountEntity> sortedQuery = sortBy?.ToLower() switch
        {
            "favorability" => sortOrder?.ToLower() == "desc"
                ? accountsQuery.OrderByDescending(a => a.Favorability)
                : accountsQuery.OrderBy(a => a.Favorability),
            "beer" => sortOrder?.ToLower() == "desc"
                ? accountsQuery.OrderByDescending(a => a.Beer)
                : accountsQuery.OrderBy(a => a.Beer),
            "username" => sortOrder?.ToLower() == "desc"
                ? accountsQuery.OrderByDescending(a => a.Username)
                : accountsQuery.OrderBy(a => a.Username),
            _ => accountsQuery.OrderBy(a => a.Id)
        };

        var accounts = await sortedQuery.Skip(skip).Take(take).ToListAsync(cancellationToken);

        _logger.LogInformation("Users retrieved. TotalCount: {TotalCount}, Returned: {Returned}", totalCount, accounts.Count);

        return (accounts, totalCount);
    }

    public async Task<AccountEntity?> FindAccountByIdAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Finding account by ID. UserId: {UserId}", userId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var account = await context.Accounts
            .Where(x => x.Id == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (account != null)
        {
            _logger.LogInformation("Account found. UserId: {UserId}, Username: {Username}", userId, account.Username);
        }
        else
        {
            _logger.LogInformation("Account not found. UserId: {UserId}", userId);
        }

        return account;
    }
}
