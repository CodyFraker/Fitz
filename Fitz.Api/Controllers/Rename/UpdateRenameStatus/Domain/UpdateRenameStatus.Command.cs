using Fitz.Api.Controllers.Rename.UpdateRenameStatus.Http;
using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.UpdateRenameStatus.Domain;

public record UpdateRenameStatusCommand(
    int RenameId,
    RenameStatusEnum Status)
{
    public static UpdateRenameStatusCommand From(int renameId, UpdateRenameStatusRequestDto request)
    {
        return new UpdateRenameStatusCommand(
            RenameId: renameId,
            Status: request.Status
        );
    }
}
