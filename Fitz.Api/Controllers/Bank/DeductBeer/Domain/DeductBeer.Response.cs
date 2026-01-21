namespace Fitz.Api.Controllers.Bank.DeductBeer.Domain;

public record DeductBeerResponse(
    ulong UserId,
    int Amount,
    int NewBalance)
{
    public static DeductBeerResponse From(DeductBeerModel model)
    {
        return new DeductBeerResponse(
            UserId: model.Account.Id,
            Amount: model.Amount,
            NewBalance: model.Account.Beer
        );
    }
}
