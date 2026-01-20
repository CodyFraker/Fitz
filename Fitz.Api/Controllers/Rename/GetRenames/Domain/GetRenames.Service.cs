namespace Fitz.Api.Controllers.Rename.GetRenames.Domain;

public class GetRenamesService(IGetRenames getRenames, ILogger<GetRenamesService> logger)
{
    private readonly IGetRenames _getRenames = getRenames;
    private readonly ILogger<GetRenamesService> _logger = logger;

    public async Task<GetRenamesModel> ExecuteAsync(GetRenamesCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetRenamesService execution started. Status: {Status}", command.Status);

        var renames = await _getRenames.GetAllRenamesAsync(command.Status, cancellationToken);

        _logger.LogInformation("GetRenamesService execution completed. Count: {Count}", renames.Count);

        return GetRenamesModel.From(renames);
    }
}
