using Fitz.Api.Controllers.Bank.GetBalance.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Bank.GetBalance.Http;

[DisplayName("GetBalanceResponse")]
public record GetBalanceResponseDto
{
    [Required]
    public required int Beer { get; set; }

    [Required]
    public required int LifetimeBeer { get; set; }

    public static GetBalanceResponseDto From(GetBalanceResponse response)
    {
        return new GetBalanceResponseDto
        {
            Beer = response.Beer,
            LifetimeBeer = response.LifetimeBeer
        };
    }
}
