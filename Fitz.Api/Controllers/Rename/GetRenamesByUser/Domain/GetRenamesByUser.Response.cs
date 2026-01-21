using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.GetRenamesByUser.Domain;

public record GetRenamesByUserResponse(
    List<RenamesEntity> Renames)
{
    public static GetRenamesByUserResponse From(GetRenamesByUserModel model)
    {
        return new GetRenamesByUserResponse(Renames: model.Renames);
    }
}
