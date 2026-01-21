namespace Fitz.Api.Controllers.Account.SetSafeBalance.Domain;

public record SetSafeBalanceResponse(
    ulong UserId,
    int SafeBalance)
{
    public static SetSafeBalanceResponse From(SetSafeBalanceModel model)
    {
        return new SetSafeBalanceResponse(
            UserId: model.Account.Id,
            SafeBalance: model.SafeBalance
        );
    }
}
