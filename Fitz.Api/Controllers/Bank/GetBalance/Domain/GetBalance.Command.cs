namespace Fitz.Api.Controllers.Bank.GetBalance.Domain;

public record GetBalanceCommand(ulong UserId, string? Username = null)
{
    public static GetBalanceCommand From(ulong userId, string? username = null)
    {
        return new GetBalanceCommand(UserId: userId, Username: username);
    }
}
