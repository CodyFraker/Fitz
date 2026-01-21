namespace Fitz.Api.Controllers.Bank.GetBalance.Domain;

public record GetBalanceResponse(
    int Beer,
    int LifetimeBeer)
{
    public static GetBalanceResponse From(GetBalanceModel model)
    {
        return new GetBalanceResponse(
            Beer: model.Account.Beer,
            LifetimeBeer: model.Account.LifetimeBeer
        );
    }
}
