using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Bank.DeductBeer.Domain;

public record DeductBeerModel(
    AccountEntity Account,
    int Amount)
{
    public static DeductBeerModel From(AccountEntity account, int amount)
    {
        return new DeductBeerModel(Account: account, Amount: amount);
    }
}
