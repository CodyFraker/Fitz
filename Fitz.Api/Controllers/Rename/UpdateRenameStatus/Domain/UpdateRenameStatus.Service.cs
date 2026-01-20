using Fitz.Api.Controllers.Rename.Exceptions;
using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.UpdateRenameStatus.Domain;

public class UpdateRenameStatusService(IUpdateRenameStatus updateRenameStatus, ILogger<UpdateRenameStatusService> logger)
{
    private readonly IUpdateRenameStatus _updateRenameStatus = updateRenameStatus;
    private readonly ILogger<UpdateRenameStatusService> _logger = logger;

    public async Task<UpdateRenameStatusModel> ExecuteAsync(UpdateRenameStatusCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("UpdateRenameStatusService execution started. RenameId: {RenameId}, Status: {Status}", command.RenameId, command.Status);

        var rename = await _updateRenameStatus.FindRenameByIdAsync(command.RenameId, cancellationToken);
        if (rename == null)
        {
            _logger.LogWarning("Rename not found. RenameId: {RenameId}", command.RenameId);
            throw new RenameNotFound(command.RenameId);
        }

        rename.Status = command.Status;
        rename.Notified = true;

        var updatedRename = await _updateRenameStatus.UpdateRenameAsync(rename, cancellationToken);

        _logger.LogInformation("UpdateRenameStatusService execution completed. RenameId: {RenameId}, Status: {Status}", command.RenameId, command.Status);

        return UpdateRenameStatusModel.From(updatedRename);
    }
}
