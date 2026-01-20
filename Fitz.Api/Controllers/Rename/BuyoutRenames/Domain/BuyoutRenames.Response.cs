namespace Fitz.Api.Controllers.Rename.BuyoutRenames.Domain;

public record BuyoutRenamesResponse(
    int RenamesUpdated)
{
    public static BuyoutRenamesResponse From(BuyoutRenamesModel model)
    {
        return new BuyoutRenamesResponse(
            RenamesUpdated: model.RenamesUpdated
        );
    }
}
