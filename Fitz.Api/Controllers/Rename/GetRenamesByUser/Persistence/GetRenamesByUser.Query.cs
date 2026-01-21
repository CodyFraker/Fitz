using Fitz.Api.Controllers.Rename.GetRenamesByUser.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Rename.GetRenamesByUser.Persistence;

public class GetRenamesByUser(IDbContextFactory<BotContext> contextFactory, ILogger<GetRenamesByUser> logger) : IGetRenamesByUser
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<GetRenamesByUser> _logger = logger;

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
