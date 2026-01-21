namespace Fitz.Api.Controllers.Rename.BuyoutRenames.Domain;

public record BuyoutRenamesResponse(
    int RenamesUpdated,
    string Message)
{
    public static BuyoutRenamesResponse From(BuyoutRenamesModel model, string message)
    {
        return new BuyoutRenamesResponse(RenamesUpdated: model.RenamesUpdated, Message: message);
    }
}
