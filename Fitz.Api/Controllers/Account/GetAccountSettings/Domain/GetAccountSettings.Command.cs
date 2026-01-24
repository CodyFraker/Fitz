namespace Fitz.Api.Controllers.Account.GetAccountSettings.Domain;

public record GetAccountSettingsCommand(ulong UserId)
{
    public static GetAccountSettingsCommand From(ulong userId)
    {
        return new GetAccountSettingsCommand(UserId: userId);
    }
}
