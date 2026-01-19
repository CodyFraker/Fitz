using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Lottery.BuyTickets.Domain;

public record BuyTicketsResponse(
    List<TicketEntity> Tickets,
    int TotalCost,
    int TicketsPurchased)
{
    public static BuyTicketsResponse From(BuyTicketsModel model)
    {
        return new BuyTicketsResponse(
            Tickets: model.Tickets,
            TotalCost: model.TotalCost,
            TicketsPurchased: model.TicketsPurchased
        );
    }
}
