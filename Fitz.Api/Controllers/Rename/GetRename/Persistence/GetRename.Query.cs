using Fitz.Api.Controllers.Rename.GetRename.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Rename.GetRename.Persistence;

public class GetRename(IDbContextFactory<BotContext> contextFactory, ILogger<GetRename> logger) : IGetRename
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<GetRename> _logger = logger;

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
}
