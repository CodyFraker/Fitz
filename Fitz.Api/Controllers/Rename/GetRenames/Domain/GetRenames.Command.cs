using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.GetRenames.Domain;

public record GetRenamesCommand(RenameStatusEnum? Status)
{
    public static GetRenamesCommand From(RenameStatusEnum? status)
    {
        return new GetRenamesCommand(Status: status);
    }
}
