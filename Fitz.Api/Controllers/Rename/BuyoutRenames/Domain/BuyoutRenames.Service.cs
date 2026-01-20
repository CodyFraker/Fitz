using Fitz.Database.Entities;
using Fitz.Metrics;

namespace Fitz.Api.Controllers.Rename.BuyoutRenames.Domain;

public class BuyoutRenamesService(IBuyoutRenames buyoutRenames, FitzMetrics? fitzMetrics, ILogger<BuyoutRenamesService> logger)
{
    private readonly IBuyoutRenames _buyoutRenames = buyoutRenames;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;
    private readonly ILogger<BuyoutRenamesService> _logger = logger;

    public async Task<BuyoutRenamesModel> ExecuteAsync(BuyoutRenamesCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("BuyoutRenamesService execution started. UserId: {UserId}", command.UserId);

        var renames = await _buyoutRenames.GetRenamesByAccountIdAsync(command.UserId, cancellationToken);

        foreach (var rename in renames)
        {
            rename.Status = RenameStatusEnum.BoughtOut;
            rename.Notified = true;
            await _buyoutRenames.UpdateRenameAsync(rename, cancellationToken);
            _fitzMetrics?.RecordRenameBoughtOut();
        }

        _logger.LogInformation("BuyoutRenamesService execution completed. UserId: {UserId}, Count: {Count}", command.UserId, renames.Count);

        return BuyoutRenamesModel.From(renames.Count);
    }
}
