using Fitz.Api.Controllers.Rename.GetRenames.Domain;
using Fitz.Api.Models.Responses;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Rename.GetRenames.Http;

[DisplayName("GetRenamesResponse")]
public record GetRenamesResponseDto
{
    [Required]
    public required List<RenameResponse> Renames { get; set; }

    public static GetRenamesResponseDto From(GetRenamesResponse response)
    {
        return new GetRenamesResponseDto
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
