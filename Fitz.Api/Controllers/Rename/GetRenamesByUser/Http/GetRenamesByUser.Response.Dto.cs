using Fitz.Api.Controllers.Rename.GetRenamesByUser.Domain;
using Fitz.Api.Models.Responses;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Rename.GetRenamesByUser.Http;

[DisplayName("GetRenamesByUserResponse")]
public record GetRenamesByUserResponseDto
{
    [Required]
    public required List<RenameResponse> Renames { get; set; }

    public static GetRenamesByUserResponseDto From(GetRenamesByUserResponse response)
    {
        return new GetRenamesByUserResponseDto
        {
            Renames = response.Renames.Select(r => new RenameResponse
            {
                Id = r.Id,
                OldName = r.OldName,
                NewName = r.NewName,
                AffectedUserId = r.AffectedUserId,
                RequestedUserId = r.RequestedUserId,
                Days = r.Days,
                Cost = r.Cost,
                Notified = r.Notified,
                Status = r.Status,
                StartDate = r.StartDate,
                Expiration = r.Expiration,
                Timestamp = r.Timestamp
            }).ToList()
        };
    }
}
