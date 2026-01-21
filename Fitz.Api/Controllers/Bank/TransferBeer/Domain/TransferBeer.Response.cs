namespace Fitz.Api.Controllers.Bank.TransferBeer.Domain;

public record TransferBeerResponse(
    ulong SenderId,
    ulong RecipientId,
    int Amount,
    int SenderNewBalance,
    int RecipientNewBalance)
{
    public static TransferBeerResponse From(TransferBeerModel model)
    {
        return new TransferBeerResponse(
            SenderId: model.SenderAccount.Id,
            RecipientId: model.RecipientAccount.Id,
            Amount: model.Amount,
            SenderNewBalance: model.SenderAccount.Beer,
            RecipientNewBalance: model.RecipientAccount.Beer
        );
    }
}
