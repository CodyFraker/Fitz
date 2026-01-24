namespace Fitz.Api.Controllers.Account.GiveBeer.Domain;

public record GiveBeerResponse(
    ulong UserId,
    int Amount,
    int NewFavorability,
    string Message)
{
    public static GiveBeerResponse From(GiveBeerModel model)
    {
        return new GiveBeerResponse(
            UserId: model.UserId,
            Amount: model.Amount,
            NewFavorability: model.NewFavorability,
            Message: model.Message
        );
    }
}
