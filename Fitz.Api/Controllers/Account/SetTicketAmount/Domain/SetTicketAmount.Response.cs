namespace Fitz.Api.Controllers.Account.SetTicketAmount.Domain;

public record SetTicketAmountResponse(
    ulong UserId,
    int Amount)
{
    public static SetTicketAmountResponse From(SetTicketAmountModel model)
    {
        return new SetTicketAmountResponse(
            UserId: model.Account.Id,
            Amount: model.Amount
        );
    }
}
