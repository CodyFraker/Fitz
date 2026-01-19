using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Lottery.BuyTickets.Domain;

public record BuyTicketsModel(
    List<TicketEntity> Tickets,
    int TotalCost,
    int TicketsPurchased)
{
    public static BuyTicketsModel From(List<TicketEntity> tickets, int totalCost, int ticketsPurchased)
    {
        return new BuyTicketsModel(
            Tickets: tickets,
            TotalCost: totalCost,
            TicketsPurchased: ticketsPurchased
        );
    }
}
