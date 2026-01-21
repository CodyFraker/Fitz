using Fitz.Api.Controllers.Rename.Exceptions;

namespace Fitz.Api.Controllers.Rename.UpdateRenameStatus.Domain;

public class UpdateRenameStatusService(IUpdateRenameStatus updateRenameStatus, ILogger<UpdateRenameStatusService> logger)
{
    private readonly IUpdateRenameStatus _updateRenameStatus = updateRenameStatus;
    private readonly ILogger<UpdateRenameStatusService> _logger = logger;

    public async Task<UpdateRenameStatusModel> ExecuteAsync(UpdateRenameStatusCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("UpdateRenameStatusService execution started. Id: {Id}, Status: {Status}", command.Id, command.Status);

        if (command.Id <= 0)
        {
            _logger.LogError("UpdateRenameStatus validation failed - Id must be greater than 0. Id: {Id}", command.Id);
            throw new ArgumentException("Id must be greater than 0.", nameof(command.Id));
        }

        var rename = await _updateRenameStatus.FindByIdAsync(command.Id, cancellationToken);
        if (rename == null)
        {
            _logger.LogWarning("Rename not found. Id: {Id}", command.Id);
            throw new RenameNotFound(command.Id);
        }

        rename.Status = command.Status;
        rename.Notified = true;

        var updatedRename = await _updateRenameStatus.UpdateRenameAsync(rename, cancellationToken);

        var model = UpdateRenameStatusModel.From(updatedRename);

        _logger.LogInformation("UpdateRenameStatusModel created successfully. Id: {Id}, Status: {Status}", command.Id, command.Status);

        return model;
    }
}
