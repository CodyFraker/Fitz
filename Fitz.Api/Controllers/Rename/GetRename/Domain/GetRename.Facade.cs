namespace Fitz.Api.Controllers.Rename.GetRename.Domain;

public class GetRenameFacade(GetRenameService getRenameService, ILogger<GetRenameFacade> logger)
{
    private readonly GetRenameService _getRenameService = getRenameService;
    private readonly ILogger<GetRenameFacade> _logger = logger;

    public async Task<GetRenameResponse> Execute(GetRenameCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetRenameFacade execution started. Id: {Id}", command.Id);

        var model = await _getRenameService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("GetRenameService execution completed. Id: {Id}", command.Id);

        var response = GetRenameResponse.From(model);

        _logger.LogInformation("GetRenameFacade execution completed successfully. Id: {Id}", command.Id);

        return response;
    }
}
