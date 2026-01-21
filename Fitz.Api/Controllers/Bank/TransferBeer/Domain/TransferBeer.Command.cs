using Fitz.Api.Controllers.Bank.TransferBeer.Http;

namespace Fitz.Api.Controllers.Bank.TransferBeer.Domain;

public record TransferBeerCommand(ulong SenderId, ulong RecipientId, int Amount)
{
    public static TransferBeerCommand From(TransferBeerRequestDto request)
    {
        return new TransferBeerCommand(SenderId: request.SenderId, RecipientId: request.RecipientId, Amount: request.Amount);
    }
}
