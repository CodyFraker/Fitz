using Fitz.Api.Controllers.Account.SetTicketAmount.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Account.SetTicketAmount.Http;

[DisplayName("SetTicketAmountResponse")]
public record SetTicketAmountResponseDto
{
    [Required]
    public required ulong UserId { get; set; }

    [Required]
    public required int Amount { get; set; }

    public static SetTicketAmountResponseDto From(SetTicketAmountResponse response)
    {
        return new SetTicketAmountResponseDto
        {
            UserId = response.UserId,
            Amount = response.Amount
        };
    }
}
