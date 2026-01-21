using Fitz.Api.Controllers.Admin.AdminModifyAccount.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Admin.AdminModifyAccount.Persistence;

public class AdminModifyAccount(IDbContextFactory<BotContext> contextFactory, ILogger<AdminModifyAccount> logger) : IAdminModifyAccount
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<AdminModifyAccount> _logger = logger;

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

    public async Task<AccountEntity> UpdateAccountAsync(AccountEntity account, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating account. UserId: {UserId}", account.Id);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.Accounts.Update(account);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Account updated successfully. UserId: {UserId}", account.Id);

        return account;
    }

    public async Task<AccountEntity?> GetAccountAfterUpdateAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting account after update. UserId: {UserId}", userId);

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
