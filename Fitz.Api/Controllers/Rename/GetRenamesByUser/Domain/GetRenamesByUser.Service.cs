namespace Fitz.Api.Controllers.Rename.GetRenamesByUser.Domain;

public class GetRenamesByUserService(IGetRenamesByUser getRenamesByUser, ILogger<GetRenamesByUserService> logger)
{
    private readonly IGetRenamesByUser _getRenamesByUser = getRenamesByUser;
    private readonly ILogger<GetRenamesByUserService> _logger = logger;

    public async Task<GetRenamesByUserModel> ExecuteAsync(GetRenamesByUserCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetRenamesByUserService execution started. UserId: {UserId}", command.UserId);

        var renames = await _getRenamesByUser.GetRenamesByAccountIdAsync(command.UserId, cancellationToken);

        _logger.LogInformation("GetRenamesByUserService execution completed. UserId: {UserId}, Count: {Count}", command.UserId, renames.Count);

        return GetRenamesByUserModel.From(renames);
    }
}
