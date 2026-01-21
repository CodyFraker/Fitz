namespace Fitz.Api.Controllers.Rename.Exceptions;

public class RenameNotFound : Exception
{
    public int RenameId { get; }

    public RenameNotFound(int renameId) : base($"Rename with ID {renameId} not found.")
    {
        RenameId = renameId;
    }
}
