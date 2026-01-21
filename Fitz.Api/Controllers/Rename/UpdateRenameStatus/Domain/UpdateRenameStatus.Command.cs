using Fitz.Api.Controllers.Rename.UpdateRenameStatus.Http;
using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.UpdateRenameStatus.Domain;

public record UpdateRenameStatusCommand(int Id, RenameStatusEnum Status)
{
    public static UpdateRenameStatusCommand From(int id, UpdateRenameStatusRequestDto request)
    {
        return new UpdateRenameStatusCommand(Id: id, Status: request.Status);
    }
}
