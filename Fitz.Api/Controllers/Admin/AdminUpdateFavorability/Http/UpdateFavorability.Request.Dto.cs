using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Admin.AdminUpdateFavorability.Http;

[DisplayName("UpdateFavorabilityRequest")]
public record UpdateFavorabilityRequestDto
{
    [Required]
    [Range(0, 100)]
    public required int Favorability { get; set; }
}
