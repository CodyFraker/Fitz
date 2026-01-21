namespace Fitz.Api.Controllers.Auth.GetCurrentUser.Domain;

public class GetCurrentUserFacade(GetCurrentUserService getCurrentUserService, ILogger<GetCurrentUserFacade> logger)
{
    private readonly GetCurrentUserService _getCurrentUserService = getCurrentUserService;
    private readonly ILogger<GetCurrentUserFacade> _logger = logger;

    public GetCurrentUserResponse Execute(GetCurrentUserCommand command)
    {
        _logger.LogInformation("GetCurrentUserFacade execution started");

        var model = _getCurrentUserService.Execute(command);

        _logger.LogInformation("GetCurrentUserService execution completed. UserId: {UserId}", model.Id);

        var response = GetCurrentUserResponse.From(model);

        _logger.LogInformation("GetCurrentUserFacade execution completed successfully. UserId: {UserId}", model.Id);

        return response;
    }
}
