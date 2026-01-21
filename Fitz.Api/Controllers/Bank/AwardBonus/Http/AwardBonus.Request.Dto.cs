using Fitz.Api.Controllers.Bank.AwardBonus.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Bank.AwardBonus.Http;

[DisplayName("AwardBonusRequest")]
public record AwardBonusRequestDto
{
    [Required]
    public required ulong UserId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public required int Amount { get; set; }

    internal AwardBonusCommand ToCommand()
    {
        return AwardBonusCommand.From(this);
    }
}
