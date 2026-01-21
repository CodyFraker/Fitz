using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.GetRenames.Domain;

public record GetRenamesResponse(
    List<RenamesEntity> Renames)
{
    public static GetRenamesResponse From(GetRenamesModel model)
    {
        return new GetRenamesResponse(Renames: model.Renames);
    }
}
