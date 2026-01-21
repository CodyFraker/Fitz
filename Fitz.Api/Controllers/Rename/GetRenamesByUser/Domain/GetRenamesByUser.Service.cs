namespace Fitz.Api.Controllers.Rename.GetRenamesByUser.Domain;

public class GetRenamesByUserService(IGetRenamesByUser getRenamesByUser, ILogger<GetRenamesByUserService> logger)
{
    private readonly IGetRenamesByUser _getRenamesByUser = getRenamesByUser;
    private readonly ILogger<GetRenamesByUserService> _logger = logger;

    public async Task<GetRenamesByUserModel> ExecuteAsync(GetRenamesByUserCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetRenamesByUserService execution started. UserId: {UserId}", command.UserId);

        if (command.UserId == 0)
        {
            _logger.LogError("GetRenamesByUser validation failed - User ID cannot be 0.");
            throw new ArgumentException("User ID cannot be 0.", nameof(command.UserId));
        }

        var renames = await _getRenamesByUser.GetRenamesByAccountIdAsync(command.UserId, cancellationToken);

        var model = GetRenamesByUserModel.From(renames);

        _logger.LogInformation("GetRenamesByUserModel created successfully. UserId: {UserId}, Count: {Count}", command.UserId, renames.Count);

        return model;
    }
}
