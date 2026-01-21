using Fitz.Api.Extensions;
using System.Security.Claims;

namespace Fitz.Api.Controllers.Auth.GetCurrentUser.Domain;

public class GetCurrentUserService(ILogger<GetCurrentUserService> logger)
{
    private readonly ILogger<GetCurrentUserService> _logger = logger;

    public GetCurrentUserModel Execute(GetCurrentUserCommand command)
    {
        _logger.LogInformation("GetCurrentUserService execution started");

        var userId = command.User.RequireDiscordUserId();
        var username = command.User.GetDiscordUsername();
        var isAdmin = command.User.IsAdmin();

        _logger.LogInformation("GetCurrentUserService - UserId: {UserId}, Username: {Username}, IsAdmin: {IsAdmin}", 
            userId, username, isAdmin);

        var model = GetCurrentUserModel.From(userId, username, isAdmin);

        _logger.LogInformation("GetCurrentUserModel created successfully. UserId: {UserId}", userId);

        return model;
    }
}
