using Fitz.Api.Controllers.Rename.CalculateRenameCost.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Rename.CalculateRenameCost.Persistence;

public class CalculateRenameCost(IDbContextFactory<BotContext> contextFactory, ILogger<CalculateRenameCost> logger) : ICalculateRenameCost
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<CalculateRenameCost> _logger = logger;

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

    public async Task<SettingsEntity?> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting settings");

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var settings = await context.Settings
            .FirstOrDefaultAsync(cancellationToken);

        if (settings != null)
        {
            _logger.LogInformation("Settings found. RenameBaseCost: {RenameBaseCost}", settings.RenameBaseCost);
        }
        else
        {
            _logger.LogWarning("Settings not found");
        }

        return settings;
    }
}
