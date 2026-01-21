using Fitz.Api.Controllers.Account.SetTicketAmount.Http;

namespace Fitz.Api.Controllers.Account.SetTicketAmount.Domain;

public record SetTicketAmountCommand(ulong UserId, int Amount)
{
    public static SetTicketAmountCommand From(SetTicketAmountRequestDto request)
    {
        return new SetTicketAmountCommand(UserId: request.UserId, Amount: request.Amount);
    }
}
