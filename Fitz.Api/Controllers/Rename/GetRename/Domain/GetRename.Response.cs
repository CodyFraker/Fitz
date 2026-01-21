using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.GetRename.Domain;

public record GetRenameResponse(
    RenamesEntity Rename)
{
    public static GetRenameResponse From(GetRenameModel model)
    {
        return new GetRenameResponse(Rename: model.Rename);
    }
}
