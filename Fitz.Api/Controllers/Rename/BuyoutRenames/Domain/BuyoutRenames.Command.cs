namespace Fitz.Api.Controllers.Rename.BuyoutRenames.Domain;

public record BuyoutRenamesCommand(ulong UserId)
{
    public static BuyoutRenamesCommand From(ulong userId)
    {
        return new BuyoutRenamesCommand(UserId: userId);
    }
}
