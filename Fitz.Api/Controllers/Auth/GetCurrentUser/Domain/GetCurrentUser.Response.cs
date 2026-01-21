namespace Fitz.Api.Controllers.Auth.GetCurrentUser.Domain;

public record GetCurrentUserResponse(
    ulong Id,
    string Username,
    bool IsAdmin)
{
    public static GetCurrentUserResponse From(GetCurrentUserModel model)
    {
        return new GetCurrentUserResponse(Id: model.Id, Username: model.Username, IsAdmin: model.IsAdmin);
    }
}
