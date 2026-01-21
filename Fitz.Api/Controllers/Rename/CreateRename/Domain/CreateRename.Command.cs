using Fitz.Api.Controllers.Rename.CreateRename.Http;
using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.CreateRename.Domain;

public record CreateRenameCommand(
    string NewName,
    ulong AffectedUserId,
    ulong RequestedUserId,
    int Days,
    DateTime? StartDate,
    DateTime? Expiration,
    RenameStatusEnum? Status)
{
    public static CreateRenameCommand From(CreateRenameRequestDto request)
    {
        return new CreateRenameCommand(
            NewName: request.NewName,
            AffectedUserId: request.AffectedUserId,
            RequestedUserId: request.RequestedUserId,
            Days: request.Days,
            StartDate: request.StartDate,
            Expiration: request.Expiration,
            Status: request.Status ?? RenameStatusEnum.Pending
        );
    }
}
