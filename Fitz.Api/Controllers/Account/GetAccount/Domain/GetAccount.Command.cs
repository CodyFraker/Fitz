namespace Fitz.Api.Controllers.Account.GetAccount.Domain;

public record GetAccountCommand(ulong UserId)
{
    public static GetAccountCommand From(ulong userId)
    {
        return new GetAccountCommand(UserId: userId);
    }
}
