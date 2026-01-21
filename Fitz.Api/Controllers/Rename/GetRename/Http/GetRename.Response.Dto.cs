using Fitz.Api.Controllers.Rename.GetRename.Domain;
using Fitz.Api.Models.Responses;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Rename.GetRename.Http;

[DisplayName("GetRenameResponse")]
public record GetRenameResponseDto
{
    [Required]
    public required RenameResponse Rename { get; set; }

    public static GetRenameResponseDto From(GetRenameResponse response)
    {
        return new GetRenameResponseDto
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
