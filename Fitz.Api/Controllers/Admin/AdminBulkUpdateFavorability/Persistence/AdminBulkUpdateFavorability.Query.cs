using Fitz.Api.Controllers.Admin.AdminBulkUpdateFavorability.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Admin.AdminBulkUpdateFavorability.Persistence;

public class AdminBulkUpdateFavorability(IDbContextFactory<BotContext> contextFactory, ILogger<AdminBulkUpdateFavorability> logger) : IAdminBulkUpdateFavorability
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<AdminBulkUpdateFavorability> _logger = logger;

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
