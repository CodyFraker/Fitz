namespace Fitz.Api.Controllers.Rename.GetRenames.Domain;

public class GetRenamesService(IGetRenames getRenames, ILogger<GetRenamesService> logger)
{
    private readonly IGetRenames _getRenames = getRenames;
    private readonly ILogger<GetRenamesService> _logger = logger;

    public async Task<GetRenamesModel> ExecuteAsync(GetRenamesCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetRenamesService execution started. Status: {Status}", command.Status);

        var renames = await _getRenames.GetAllRenamesAsync(command.Status, cancellationToken);

        var model = GetRenamesModel.From(renames);

        _logger.LogInformation("GetRenamesModel created successfully. Count: {Count}", renames.Count);

        return model;
    }
}
