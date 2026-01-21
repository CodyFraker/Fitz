using Fitz.Api.Controllers.Rename.BuyoutRenames.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Rename.BuyoutRenames.Persistence;

public class BuyoutRenames(IDbContextFactory<BotContext> contextFactory, ILogger<BuyoutRenames> logger) : IBuyoutRenames
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<BuyoutRenames> _logger = logger;

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

    public async Task<RenamesEntity> UpdateRenameAsync(RenamesEntity rename, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating rename. Id: {Id}, Status: {Status}", rename.Id, rename.Status);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.Renames.Update(rename);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Rename updated successfully. Id: {Id}, Status: {Status}", rename.Id, rename.Status);

        return rename;
    }
}
