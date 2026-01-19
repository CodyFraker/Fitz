using Fitz.Api.Controllers.Account.GetAccount.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Account.GetAccount.Persistence;

public class GetAccount(IDbContextFactory<BotContext> contextFactory, ILogger<GetAccount> logger) : IGetAccount
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<GetAccount> _logger = logger;

    public async Task<AccountEntity?> FindByIdAsync(ulong id, CancellationToken cancellationToken = default)
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
}
