namespace Fitz.Api.Controllers.Rename.GetRename.Domain;

public record GetRenameCommand(int Id)
{
    public static GetRenameCommand From(int id)
    {
        return new GetRenameCommand(Id: id);
    }
}
