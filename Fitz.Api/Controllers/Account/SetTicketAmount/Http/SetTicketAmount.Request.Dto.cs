using Fitz.Api.Controllers.Account.SetTicketAmount.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Account.SetTicketAmount.Http;

[DisplayName("SetTicketAmountRequest")]
public record SetTicketAmountRequestDto
{
    [Required]
    public required ulong UserId { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public required int Amount { get; set; }

    internal SetTicketAmountCommand ToCommand()
    {
        return SetTicketAmountCommand.From(this);
    }
}
