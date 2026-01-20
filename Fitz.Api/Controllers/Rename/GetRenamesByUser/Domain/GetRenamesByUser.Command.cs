namespace Fitz.Api.Controllers.Rename.GetRenamesByUser.Domain;

public record GetRenamesByUserCommand(ulong UserId)
{
    public static GetRenamesByUserCommand From(ulong userId)
    {
        return new GetRenamesByUserCommand(UserId: userId);
    }
}
