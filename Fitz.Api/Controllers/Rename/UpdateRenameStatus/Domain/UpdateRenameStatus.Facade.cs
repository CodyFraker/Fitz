namespace Fitz.Api.Controllers.Rename.UpdateRenameStatus.Domain;

public class UpdateRenameStatusFacade(UpdateRenameStatusService updateRenameStatusService, ILogger<UpdateRenameStatusFacade> logger)
{
    private readonly UpdateRenameStatusService _updateRenameStatusService = updateRenameStatusService;
    private readonly ILogger<UpdateRenameStatusFacade> _logger = logger;

    public async Task<UpdateRenameStatusResponse> Execute(UpdateRenameStatusCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("UpdateRenameStatusFacade execution started. RenameId: {RenameId}, Status: {Status}", command.RenameId, command.Status);

        var model = await _updateRenameStatusService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("UpdateRenameStatusService execution completed. RenameId: {RenameId}", model.Rename.Id);

        var response = UpdateRenameStatusResponse.From(model);

        _logger.LogInformation("UpdateRenameStatusFacade execution completed successfully. RenameId: {RenameId}, Status: {Status}", command.RenameId, command.Status);

        return response;
    }
}
