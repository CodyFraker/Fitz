using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Account.SetTicketAmount.Domain;

public record SetTicketAmountModel(
    AccountEntity Account,
    int Amount)
{
    public static SetTicketAmountModel From(AccountEntity account, int amount)
    {
        return new SetTicketAmountModel(
            Account: account,
            Amount: amount
        );
    }
}
