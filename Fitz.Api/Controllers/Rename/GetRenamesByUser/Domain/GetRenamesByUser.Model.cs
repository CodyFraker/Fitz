using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.GetRenamesByUser.Domain;

public record GetRenamesByUserModel(
    List<RenamesEntity> Renames)
{
    public static GetRenamesByUserModel From(List<RenamesEntity> renames)
    {
        return new GetRenamesByUserModel(Renames: renames);
    }
}
