namespace Fitz.Api.Controllers.Account.GiveBeer.Domain;

public record GiveBeerModel(
    ulong UserId,
    int Amount,
    int NewFavorability,
    string Message)
{
    public static GiveBeerModel From(ulong userId, int amount, int newFavorability, string message)
    {
        return new GiveBeerModel(
            UserId: userId,
            Amount: amount,
            NewFavorability: newFavorability,
            Message: message
        );
    }
}
