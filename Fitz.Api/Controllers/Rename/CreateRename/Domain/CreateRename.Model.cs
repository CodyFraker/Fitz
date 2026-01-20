using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.CreateRename.Domain;

public record CreateRenameModel(
    RenamesEntity Rename)
{
    public static CreateRenameModel From(RenamesEntity rename)
    {
        return new CreateRenameModel(
            Rename: rename
        );
    }
}
