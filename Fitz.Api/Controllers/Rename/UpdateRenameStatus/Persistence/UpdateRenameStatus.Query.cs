using Fitz.Api.Controllers.Rename.UpdateRenameStatus.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Rename.UpdateRenameStatus.Persistence;

public class UpdateRenameStatus(IDbContextFactory<BotContext> contextFactory, ILogger<UpdateRenameStatus> logger) : IUpdateRenameStatus
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<UpdateRenameStatus> _logger = logger;

    public async Task<RenamesEntity?> FindRenameByIdAsync(int renameId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Finding rename by ID. RenameId: {RenameId}", renameId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var rename = await context.Renames
            .Where(r => r.Id == renameId)
            .FirstOrDefaultAsync(cancellationToken);

        if (rename != null)
        {
            _logger.LogInformation("Rename found. RenameId: {RenameId}", renameId);
        }
        else
        {
            _logger.LogInformation("Rename not found. RenameId: {RenameId}", renameId);
        }

        return rename;
    }

    public async Task<RenamesEntity> UpdateRenameAsync(RenamesEntity rename, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating rename. RenameId: {RenameId}, Status: {Status}", rename.Id, rename.Status);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.Renames.Update(rename);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Rename updated successfully. RenameId: {RenameId}", rename.Id);

        return rename;
    }
}
