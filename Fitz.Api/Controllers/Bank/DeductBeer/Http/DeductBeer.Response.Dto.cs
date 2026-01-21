using Fitz.Api.Controllers.Bank.DeductBeer.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Bank.DeductBeer.Http;

[DisplayName("DeductBeerResponse")]
public record DeductBeerResponseDto
{
    [Required]
    public required ulong UserId { get; set; }

    [Required]
    public required int Amount { get; set; }

    [Required]
    public required int NewBalance { get; set; }

    public static DeductBeerResponseDto From(DeductBeerResponse response)
    {
        return new DeductBeerResponseDto
        {
            UserId = response.UserId,
            Amount = response.Amount,
            NewBalance = response.NewBalance
        };
    }
}
