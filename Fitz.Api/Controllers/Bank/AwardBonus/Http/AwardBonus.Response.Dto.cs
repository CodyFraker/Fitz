using Fitz.Api.Controllers.Bank.AwardBonus.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Bank.AwardBonus.Http;

[DisplayName("AwardBonusResponse")]
public record AwardBonusResponseDto
{
    [Required]
    public required ulong UserId { get; set; }

    [Required]
    public required int Amount { get; set; }

    [Required]
    public required int NewBalance { get; set; }

    public static AwardBonusResponseDto From(AwardBonusResponse response)
    {
        return new AwardBonusResponseDto
        {
            UserId = response.UserId,
            Amount = response.Amount,
            NewBalance = response.NewBalance
        };
    }
}
