using Fitz.Api.Controllers.Rename.GetRenames.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Rename.GetRenames.Persistence;

public class GetRenames(IDbContextFactory<BotContext> contextFactory, ILogger<GetRenames> logger) : IGetRenames
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<GetRenames> _logger = logger;

    public async Task<List<RenamesEntity>> GetAllRenamesAsync(RenameStatusEnum? status, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all renames. Status: {Status}", status);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        IQueryable<RenamesEntity> query = context.Renames;

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        var renames = await query.ToListAsync(cancellationToken);

        _logger.LogInformation("Renames retrieved. Count: {Count}", renames.Count);

        return renames;
    }
}
