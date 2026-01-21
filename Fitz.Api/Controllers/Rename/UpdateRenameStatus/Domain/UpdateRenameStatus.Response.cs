using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.UpdateRenameStatus.Domain;

public record UpdateRenameStatusResponse(
    RenamesEntity Rename,
    string Message)
{
    public static UpdateRenameStatusResponse From(UpdateRenameStatusModel model, string message)
    {
        return new UpdateRenameStatusResponse(Rename: model.Rename, Message: message);
    }
}
