using Fitz.Api.Controllers.Rename.CreateRename.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Rename.CreateRename.Persistence;

public class CreateRename(IDbContextFactory<BotContext> contextFactory, ILogger<CreateRename> logger) : ICreateRename
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<CreateRename> _logger = logger;

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
    public async Task<RenamesEntity> CreateRenameAsync(RenamesEntity rename, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating rename. AffectedUserId: {AffectedUserId}, RequestedUserId: {RequestedUserId}, NewName: {NewName}", 
            rename.AffectedUserId, rename.RequestedUserId, rename.NewName);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.Renames.Add(rename);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Rename created successfully. RenameId: {RenameId}", rename.Id);

        return rename;
    }

    public async Task<RenamesEntity?> FindRenameAfterCreationAsync(ulong affectedUserId, ulong requestedUserId, string newName, DateTime timestamp, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Finding rename after creation. AffectedUserId: {AffectedUserId}, RequestedUserId: {RequestedUserId}, NewName: {NewName}", 
            affectedUserId, requestedUserId, newName);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var rename = await context.Renames
            .Where(r => r.AffectedUserId == affectedUserId 
                && r.RequestedUserId == requestedUserId 
                && r.NewName == newName
                && r.Timestamp >= timestamp.AddSeconds(-5))
            .OrderByDescending(r => r.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);

        if (rename != null)
        {
            _logger.LogInformation("Rename found. RenameId: {RenameId}", rename.Id);
        }
        else
        {
            _logger.LogWarning("Rename not found after creation");
        }

        return rename;
    }

    public async Task<List<RenamesEntity>> GetRenamesByAccountIdAsync(ulong accountId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting renames by account ID. AccountId: {AccountId}", accountId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var renames = await context.Renames
            .Where(r => r.AffectedUserId == accountId && (r.Status == RenameStatusEnum.Pending || r.Status == RenameStatusEnum.Active))
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Renames retrieved. AccountId: {AccountId}, Count: {Count}", accountId, renames.Count);

        return renames;
    }
}
