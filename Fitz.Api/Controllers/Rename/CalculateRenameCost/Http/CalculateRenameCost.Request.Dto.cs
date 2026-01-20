using Fitz.Api.Controllers.Rename.CalculateRenameCost.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Rename.CalculateRenameCost.Http;

[DisplayName("CalculateRenameCostRequest")]
public record CalculateRenameCostRequestDto
{
    [Required]
    public required ulong AffectedUserId { get; set; }

    [Required]
    public required ulong RequestedUserId { get; set; }

    [Required]
    [Range(1, 365)]
    public required double Days { get; set; }

    [Required]
    [MaxLength(32)]
    public required string NewName { get; set; }

    internal CalculateRenameCostCommand ToCommand()
    {
        return CalculateRenameCostCommand.From(this);
    }
}
