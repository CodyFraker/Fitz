using Fitz.Api.Controllers.Bank.DeductBeer.Http;
using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Bank.DeductBeer.Domain;

public record DeductBeerCommand(ulong UserId, int Amount, Reason Reason)
{
    public static DeductBeerCommand From(DeductBeerRequestDto request)
    {
        if (!Enum.TryParse<Reason>(request.Reason, out var reason))
        {
            throw new ArgumentException($"Invalid reason: {request.Reason}", nameof(request.Reason));
        }

        return new DeductBeerCommand(UserId: request.UserId, Amount: request.Amount, Reason: reason);
    }
}
