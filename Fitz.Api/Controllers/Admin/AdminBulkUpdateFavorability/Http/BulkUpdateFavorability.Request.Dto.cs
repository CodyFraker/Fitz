using Fitz.Api.Controllers.Admin.AdminBulkUpdateFavorability.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Admin.AdminBulkUpdateFavorability.Http;

[DisplayName("BulkUpdateFavorabilityRequest")]
public record BulkUpdateFavorabilityRequestDto
{
    [Required]
    public required ulong[] UserIds { get; set; }

    [Required]
    [Range(0, 100)]
    public required int Favorability { get; set; }

    internal AdminBulkUpdateFavorabilityCommand ToCommand()
    {
        return AdminBulkUpdateFavorabilityCommand.From(this);
    }
}
