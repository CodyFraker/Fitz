using Fitz.Api.Controllers.Bank.DeductBeer.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Bank.DeductBeer.Http;

[DisplayName("DeductBeerRequest")]
public record DeductBeerRequestDto
{
    [Required]
    public required ulong UserId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public required int Amount { get; set; }

    [Required]
    public required string Reason { get; set; }

    internal DeductBeerCommand ToCommand()
    {
        return DeductBeerCommand.From(this);
    }
}
