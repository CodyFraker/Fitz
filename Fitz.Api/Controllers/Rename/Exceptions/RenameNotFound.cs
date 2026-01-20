namespace Fitz.Api.Controllers.Rename.Exceptions;

public class RenameNotFound : Exception
{
    public RenameNotFound() : base("Rename not found")
    {
    }

    public RenameNotFound(int renameId) : base($"Rename with ID {renameId} not found")
    {
    }
}
