using Fitz.Api.Controllers.Rename.CreateRename.Domain;
using Fitz.Api.Models.Responses;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Rename.CreateRename.Http;

[DisplayName("CreateRenameResponse")]
public record CreateRenameResponseDto
{
    [Required]
    public required RenameResponse Rename { get; set; }

    public static CreateRenameResponseDto From(CreateRenameResponse response)
    {
        return new CreateRenameResponseDto
        {
            Rename = new RenameResponse
            {
                Id = response.Rename.Id,
                OldName = response.Rename.OldName,
                NewName = response.Rename.NewName,
                AffectedUserId = response.Rename.AffectedUserId,
                RequestedUserId = response.Rename.RequestedUserId,
                Days = response.Rename.Days,
                Cost = response.Rename.Cost,
                Notified = response.Rename.Notified,
                Status = response.Rename.Status,
                StartDate = response.Rename.StartDate,
                Expiration = response.Rename.Expiration,
                Timestamp = response.Rename.Timestamp
            }
        };
    }
}
