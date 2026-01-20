namespace Fitz.Api.Controllers.Rename.CreateRename.Domain;

public class CreateRenameFacade(CreateRenameService createRenameService, ILogger<CreateRenameFacade> logger)
{
    private readonly CreateRenameService _createRenameService = createRenameService;
    private readonly ILogger<CreateRenameFacade> _logger = logger;

    public async Task<CreateRenameResponse> Execute(CreateRenameCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("CreateRenameFacade execution started. AffectedUserId: {AffectedUserId}, RequestedUserId: {RequestedUserId}", command.AffectedUserId, command.RequestedUserId);

        var model = await _createRenameService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("CreateRenameService execution completed. RenameId: {RenameId}", model.Rename.Id);

        var response = CreateRenameResponse.From(model);

        _logger.LogInformation("CreateRenameFacade execution completed successfully. RenameId: {RenameId}", model.Rename.Id);

        return response;
    }
}
