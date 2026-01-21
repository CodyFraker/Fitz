using Fitz.Api.Controllers.Account.SetSafeBalance.Http;

namespace Fitz.Api.Controllers.Account.SetSafeBalance.Domain;

public record SetSafeBalanceCommand(ulong UserId, int SafeBalance)
{
    public static SetSafeBalanceCommand From(SetSafeBalanceRequestDto request)
    {
        return new SetSafeBalanceCommand(UserId: request.UserId, SafeBalance: request.SafeBalance);
    }
}
