using Fitz.Api.Controllers.Account.GiveBeer.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fitz.Api.Controllers.Account.GiveBeer.Persistence;

public class GiveBeer(IDbContextFactory<BotContext> contextFactory, IServiceScopeFactory scopeFactory, ILogger<GiveBeer> logger) : IGiveBeer
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<GiveBeer> _logger = logger;

    public async Task<AccountEntity?> FindAccountByIdAsync(ulong id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Finding account by ID. AccountId: {AccountId}", id);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var account = await context.Accounts
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (account != null)
        {
            _logger.LogInformation("Account found by ID. AccountId: {AccountId}, Username: {Username}", id, account.Username);
        }
        else
        {
            _logger.LogInformation("Account not found by ID. AccountId: {AccountId}", id);
        }

        return account;
    }

    public IServiceScopeFactory GetScopeFactory()
    {
        return _scopeFactory;
    }
}
