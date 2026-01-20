using Fitz.Api.Controllers.Rename.GetRenamesByUser.Domain;
using Fitz.Database.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Rename.GetRenamesByUser.Http;

[DisplayName("GetRenamesByUserResponse")]
public record GetRenamesByUserResponseDto
{
    [Required]
    public required List<RenameResponseItemDto> Renames { get; set; }

    public static GetRenamesByUserResponseDto From(GetRenamesByUserResponse response)
    {
        return new GetRenamesByUserResponseDto
        {
            Renames = response.Renames.Select(r => new RenameResponseItemDto
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

public record RenameResponseItemDto
{
    [Required]
    public required int Id { get; set; }

    public string? OldName { get; set; }

    [Required]
    public required string NewName { get; set; }

    [Required]
    public required ulong AffectedUserId { get; set; }

    [Required]
    public required ulong RequestedUserId { get; set; }

    public int? Days { get; set; }

    [Required]
    public required int Cost { get; set; }

    [Required]
    public required bool Notified { get; set; }

    [Required]
    public required RenameStatusEnum Status { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? Expiration { get; set; }

    [Required]
    public required DateTime Timestamp { get; set; }
}
