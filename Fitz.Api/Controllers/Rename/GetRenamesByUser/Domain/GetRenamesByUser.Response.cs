using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.GetRenamesByUser.Domain;

public record GetRenamesByUserResponse(
    List<RenameResponseItem> Renames)
{
    public static GetRenamesByUserResponse From(GetRenamesByUserModel model)
    {
        return new GetRenamesByUserResponse(
            Renames: model.Renames.Select(r => new RenameResponseItem(
                Id: r.Id,
                OldName: r.OldName,
                NewName: r.NewName,
                AffectedUserId: r.AffectedUserId,
                RequestedUserId: r.RequestedUserId,
                Days: r.Days,
                Cost: r.Cost,
                Notified: r.Notified,
                Status: r.Status,
                StartDate: r.StartDate,
                Expiration: r.Expiration,
                Timestamp: r.Timestamp
            )).ToList()
        );
    }
}

public record RenameResponseItem(
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
    DateTime Timestamp);
