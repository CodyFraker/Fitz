using Fitz.Api.Controllers.Rename.Exceptions;

namespace Fitz.Api.Controllers.Rename.GetRename.Domain;

public class GetRenameService(IGetRename getRename, ILogger<GetRenameService> logger)
{
    private readonly IGetRename _getRename = getRename;
    private readonly ILogger<GetRenameService> _logger = logger;

    public async Task<GetRenameModel> ExecuteAsync(GetRenameCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetRenameService execution started. Id: {Id}", command.Id);

        if (command.Id <= 0)
        {
            _logger.LogError("GetRename validation failed - Id must be greater than 0. Id: {Id}", command.Id);
            throw new ArgumentException("Id must be greater than 0.", nameof(command.Id));
        }

        var rename = await _getRename.FindByIdAsync(command.Id, cancellationToken);
        if (rename == null)
        {
            _logger.LogWarning("Rename not found. Id: {Id}", command.Id);
            throw new RenameNotFound(command.Id);
        }

        var model = GetRenameModel.From(rename);

        _logger.LogInformation("GetRenameModel created successfully. Id: {Id}", command.Id);

        return model;
    }
}
