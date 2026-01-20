using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.GetRenames.Domain;

public record GetRenamesModel(
    List<RenamesEntity> Renames)
{
    public static GetRenamesModel From(List<RenamesEntity> renames)
    {
        return new GetRenamesModel(
            Renames: renames
        );
    }
}
