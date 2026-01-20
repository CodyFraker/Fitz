using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.CreateRename.Domain;

public record CreateRenameResponse(
    int Id,
    string? OldName,
    string NewName,
    ulong AffectedUserId,
    ulong RequestedUserId,
    int? Days,
    int Cost,
    bool Notified,
    RenameStatusEnum Status,
    DateTime? StartDate,
    DateTime? Expiration,
    DateTime Timestamp)
{
    public static CreateRenameResponse From(CreateRenameModel model)
    {
        return new CreateRenameResponse(
            Id: model.Rename.Id,
            OldName: model.Rename.OldName,
            NewName: model.Rename.NewName,
            AffectedUserId: model.Rename.AffectedUserId,
            RequestedUserId: model.Rename.RequestedUserId,
            Days: model.Rename.Days,
            Cost: model.Rename.Cost,
            Notified: model.Rename.Notified,
            Status: model.Rename.Status,
            StartDate: model.Rename.StartDate,
            Expiration: model.Rename.Expiration,
            Timestamp: model.Rename.Timestamp
        );
    }
}
