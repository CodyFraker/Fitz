namespace Fitz.Api.Controllers.Rename.GetRename.Domain;

public record GetRenameCommand(int RenameId)
{
    public static GetRenameCommand FromId(int renameId)
    {
        return new GetRenameCommand(RenameId: renameId);
    }
}
