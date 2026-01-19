using Fitz.Api.Controllers.Lottery.BuyTickets.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Lottery.BuyTickets.Http;

[DisplayName("BuyTicketsResponse")]
public record BuyTicketsResponseDto
{
    [Required]
    public required List<TicketInfoDto> Tickets { get; set; }

    [Required]
    public required int TotalCost { get; set; }

    [Required]
    public required int TicketsPurchased { get; set; }

    public static BuyTicketsResponseDto From(BuyTicketsResponse response)
    {
        return new BuyTicketsResponseDto
        {
            Tickets = response.Tickets.Select(t => new TicketInfoDto
            {
                Id = t.Id,
                Number = t.Number,
                Drawing = t.Drawing,
                Timestamp = t.Timestamp
            }).ToList(),
            TotalCost = response.TotalCost,
            TicketsPurchased = response.TicketsPurchased
        };
    }
}

public record TicketInfoDto
{
    [Required]
    public required int Id { get; set; }

    [Required]
    public required int Number { get; set; }

    [Required]
    public required int Drawing { get; set; }

    [Required]
    public required DateTime Timestamp { get; set; }
}
