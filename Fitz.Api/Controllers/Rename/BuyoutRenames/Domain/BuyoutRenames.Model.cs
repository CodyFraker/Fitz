namespace Fitz.Api.Controllers.Rename.BuyoutRenames.Domain;

public record BuyoutRenamesModel(
    int RenamesUpdated)
{
    public static BuyoutRenamesModel From(int renamesUpdated)
    {
        return new BuyoutRenamesModel(
            RenamesUpdated: renamesUpdated
        );
    }
}
