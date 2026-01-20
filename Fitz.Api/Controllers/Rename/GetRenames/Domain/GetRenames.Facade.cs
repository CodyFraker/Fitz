namespace Fitz.Api.Controllers.Rename.GetRenames.Domain;

public class GetRenamesFacade(GetRenamesService getRenamesService, ILogger<GetRenamesFacade> logger)
{
    private readonly GetRenamesService _getRenamesService = getRenamesService;
    private readonly ILogger<GetRenamesFacade> _logger = logger;

    public async Task<GetRenamesResponse> Execute(GetRenamesCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetRenamesFacade execution started");

        var model = await _getRenamesService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("GetRenamesService execution completed. Count: {Count}", model.Renames.Count);

        var response = GetRenamesResponse.From(model);

        _logger.LogInformation("GetRenamesFacade execution completed successfully. Count: {Count}", model.Renames.Count);

        return response;
    }
}
