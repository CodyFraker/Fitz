using Fitz.Database.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Rename.UpdateRenameStatus.Http;

[DisplayName("UpdateRenameStatusRequest")]
public record UpdateRenameStatusRequestDto
{
    [Required]
    public required RenameStatusEnum Status { get; set; }
}
