using Fitz.Api.Controllers.Rename.UpdateRenameStatus.Domain;
using Fitz.Api.Models.Responses;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Rename.UpdateRenameStatus.Http;

[DisplayName("UpdateRenameStatusResponse")]
public record UpdateRenameStatusResponseDto
{
    [Required]
    public required RenameResponse Rename { get; set; }

    public static UpdateRenameStatusResponseDto From(UpdateRenameStatusResponse response)
    {
        return new UpdateRenameStatusResponseDto
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
