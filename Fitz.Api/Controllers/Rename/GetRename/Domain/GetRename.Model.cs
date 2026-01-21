using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.GetRename.Domain;

public record GetRenameModel(
    RenamesEntity Rename)
{
    public static GetRenameModel From(RenamesEntity rename)
    {
        return new GetRenameModel(Rename: rename);
    }
}
