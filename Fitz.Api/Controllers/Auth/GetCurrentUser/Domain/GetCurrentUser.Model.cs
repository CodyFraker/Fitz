namespace Fitz.Api.Controllers.Auth.GetCurrentUser.Domain;

public record GetCurrentUserModel(
    ulong Id,
    string Username,
    bool IsAdmin)
{
    public static GetCurrentUserModel From(ulong id, string username, bool isAdmin)
    {
        return new GetCurrentUserModel(Id: id, Username: username, IsAdmin: isAdmin);
    }
}
