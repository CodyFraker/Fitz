using Fitz.Api.Controllers.Rename.Exceptions;

namespace Fitz.Api.Controllers.Rename.GetRename.Domain;

public class GetRenameService(IGetRename getRename, ILogger<GetRenameService> logger)
{
    private readonly IGetRename _getRename = getRename;
    private readonly ILogger<GetRenameService> _logger = logger;

    public async Task<GetRenameModel> ExecuteAsync(GetRenameCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetRenameService execution started. RenameId: {RenameId}", command.RenameId);

        var rename = await _getRename.FindRenameByIdAsync(command.RenameId, cancellationToken);

        if (rename == null)
        {
            _logger.LogWarning("Rename not found. RenameId: {RenameId}", command.RenameId);
            throw new RenameNotFound(command.RenameId);
        }

        _logger.LogInformation("GetRenameService execution completed. RenameId: {RenameId}", command.RenameId);

        return GetRenameModel.From(rename);
    }
}
