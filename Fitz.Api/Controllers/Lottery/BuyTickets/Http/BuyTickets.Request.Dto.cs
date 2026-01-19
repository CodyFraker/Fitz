using Fitz.Api.Controllers.Lottery.BuyTickets.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Lottery.BuyTickets.Http;

[DisplayName("BuyTicketsRequest")]
public record BuyTicketsRequestDto
{
    [Required]
    public required ulong UserId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public required int Amount { get; set; }

    internal BuyTicketsCommand ToCommand()
    {
        return BuyTicketsCommand.From(this);
    }
}
