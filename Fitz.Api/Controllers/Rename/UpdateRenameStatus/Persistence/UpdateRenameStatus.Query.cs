using Fitz.Api.Controllers.Rename.UpdateRenameStatus.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Rename.UpdateRenameStatus.Persistence;

public class UpdateRenameStatus(IDbContextFactory<BotContext> contextFactory, ILogger<UpdateRenameStatus> logger) : IUpdateRenameStatus
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<UpdateRenameStatus> _logger = logger;

    public async Task<RenamesEntity?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Finding rename by ID. Id: {Id}", id);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var rename = await context.Renames
            .Where(r => r.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (rename != null)
        {
            _logger.LogInformation("Rename found. Id: {Id}, NewName: {NewName}", id, rename.NewName);
        }
        else
        {
            _logger.LogInformation("Rename not found. Id: {Id}", id);
        }

        return rename;
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
