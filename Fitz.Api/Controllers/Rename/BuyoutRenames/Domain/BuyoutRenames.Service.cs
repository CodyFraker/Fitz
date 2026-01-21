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

        if (command.UserId == 0)
        {
            _logger.LogError("BuyoutRenames validation failed - User ID cannot be 0.");
            throw new ArgumentException("User ID cannot be 0.", nameof(command.UserId));
        }

        var renames = await _buyoutRenames.GetRenamesByAccountIdAsync(command.UserId, cancellationToken);

        int updatedCount = 0;
        foreach (var rename in renames)
        {
            rename.Status = RenameStatusEnum.BoughtOut;
            rename.Notified = true;
            await _buyoutRenames.UpdateRenameAsync(rename, cancellationToken);
            _fitzMetrics?.RecordRenameBoughtOut();
            updatedCount++;
        }

        var model = BuyoutRenamesModel.From(updatedCount);

        _logger.LogInformation("BuyoutRenamesModel created successfully. UserId: {UserId}, UpdatedCount: {UpdatedCount}", command.UserId, updatedCount);

        return model;
    }
}
