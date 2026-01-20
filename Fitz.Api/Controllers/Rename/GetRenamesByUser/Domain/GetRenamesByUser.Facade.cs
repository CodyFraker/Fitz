namespace Fitz.Api.Controllers.Rename.GetRenamesByUser.Domain;

public class GetRenamesByUserFacade(GetRenamesByUserService getRenamesByUserService, ILogger<GetRenamesByUserFacade> logger)
{
    private readonly GetRenamesByUserService _getRenamesByUserService = getRenamesByUserService;
    private readonly ILogger<GetRenamesByUserFacade> _logger = logger;

    public async Task<GetRenamesByUserResponse> Execute(GetRenamesByUserCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetRenamesByUserFacade execution started. UserId: {UserId}", command.UserId);

        var model = await _getRenamesByUserService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("GetRenamesByUserService execution completed. UserId: {UserId}, Count: {Count}", command.UserId, model.Renames.Count);

        var response = GetRenamesByUserResponse.From(model);

        _logger.LogInformation("GetRenamesByUserFacade execution completed successfully. UserId: {UserId}, Count: {Count}", command.UserId, model.Renames.Count);

        return response;
    }
}
