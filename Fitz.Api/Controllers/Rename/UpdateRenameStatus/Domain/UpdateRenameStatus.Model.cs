using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.UpdateRenameStatus.Domain;

public record UpdateRenameStatusModel(
    RenamesEntity Rename)
{
    public static UpdateRenameStatusModel From(RenamesEntity rename)
    {
        return new UpdateRenameStatusModel(Rename: rename);
    }
}
