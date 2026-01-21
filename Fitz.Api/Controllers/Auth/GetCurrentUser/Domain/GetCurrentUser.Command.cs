using System.Security.Claims;

namespace Fitz.Api.Controllers.Auth.GetCurrentUser.Domain;

public record GetCurrentUserCommand(ClaimsPrincipal User)
{
    public static GetCurrentUserCommand From(ClaimsPrincipal user)
    {
        return new GetCurrentUserCommand(User: user);
    }
}
