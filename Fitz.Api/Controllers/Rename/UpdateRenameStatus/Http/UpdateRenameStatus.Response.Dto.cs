using Fitz.Api.Controllers.Rename.UpdateRenameStatus.Domain;
using Fitz.Database.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Rename.UpdateRenameStatus.Http;

[DisplayName("UpdateRenameStatusResponse")]
public record UpdateRenameStatusResponseDto
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

    public static UpdateRenameStatusResponseDto From(UpdateRenameStatusResponse response)
    {
        return new UpdateRenameStatusResponseDto
        {
            Id = response.Id,
            OldName = response.OldName,
            NewName = response.NewName,
            AffectedUserId = response.AffectedUserId,
            RequestedUserId = response.RequestedUserId,
            Days = response.Days,
            Cost = response.Cost,
            Notified = response.Notified,
            Status = response.Status,
            StartDate = response.StartDate,
            Expiration = response.Expiration,
            Timestamp = response.Timestamp
        };
    }
}
