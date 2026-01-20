using Fitz.Api.Controllers.Rename.CalculateRenameCost.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Rename.CalculateRenameCost.Http;

[DisplayName("CalculateRenameCostResponse")]
public record CalculateRenameCostResponseDto
{
    [Required]
    public required int Cost { get; set; }

    public static CalculateRenameCostResponseDto From(CalculateRenameCostResponse response)
    {
        return new CalculateRenameCostResponseDto
        {
            Cost = response.Cost
        };
    }
}
