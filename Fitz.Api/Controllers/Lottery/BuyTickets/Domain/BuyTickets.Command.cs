using Fitz.Api.Controllers.Lottery.BuyTickets.Http;

namespace Fitz.Api.Controllers.Lottery.BuyTickets.Domain;

public record BuyTicketsCommand(ulong UserId, int Amount)
{
    public static BuyTicketsCommand From(BuyTicketsRequestDto request)
    {
        return new BuyTicketsCommand(UserId: request.UserId, Amount: request.Amount);
    }
}
