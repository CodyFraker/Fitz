using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.CreateRename.Domain;

public record CreateRenameResponse(
    RenamesEntity Rename,
    string Message)
{
    public static CreateRenameResponse From(CreateRenameModel model, string message)
    {
        return new CreateRenameResponse(Rename: model.Rename, Message: message);
    }
}
