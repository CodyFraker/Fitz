namespace Fitz.Api.Controllers.Rename.UpdateRenameStatus.Domain;

public class UpdateRenameStatusFacade(UpdateRenameStatusService updateRenameStatusService, ILogger<UpdateRenameStatusFacade> logger)
{
    private readonly UpdateRenameStatusService _updateRenameStatusService = updateRenameStatusService;
    private readonly ILogger<UpdateRenameStatusFacade> _logger = logger;

    public async Task<UpdateRenameStatusResponse> Execute(UpdateRenameStatusCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("UpdateRenameStatusFacade execution started. Id: {Id}, Status: {Status}", command.Id, command.Status);

        var model = await _updateRenameStatusService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("UpdateRenameStatusService execution completed. Id: {Id}, Status: {Status}", command.Id, command.Status);

        var response = UpdateRenameStatusResponse.From(model, "Successfully updated rename status.");

        _logger.LogInformation("UpdateRenameStatusFacade execution completed successfully. Id: {Id}, Status: {Status}", command.Id, command.Status);

        return response;
    }
}
