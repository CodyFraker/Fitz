using Fitz.Api.Controllers.Rename.CreateRename.Domain;
using Fitz.Database.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Rename.CreateRename.Http;

[DisplayName("CreateRenameRequest")]
public record CreateRenameRequestDto
{
    [Required]
    [MaxLength(32)]
    public required string NewName { get; set; }

    [Required]
    public required ulong AffectedUserId { get; set; }

    [Required]
    public required ulong RequestedUserId { get; set; }

    [Required]
    [Range(1, 365)]
    public required int Days { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? Expiration { get; set; }

    public RenameStatusEnum? Status { get; set; }

    internal CreateRenameCommand ToCommand()
    {
        return CreateRenameCommand.From(this);
    }
}
