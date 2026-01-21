using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Bank.TransferBeer.Domain;

public record TransferBeerModel(
    AccountEntity SenderAccount,
    AccountEntity RecipientAccount,
    int Amount)
{
    public static TransferBeerModel From(AccountEntity senderAccount, AccountEntity recipientAccount, int amount)
    {
        return new TransferBeerModel(
            SenderAccount: senderAccount,
            RecipientAccount: recipientAccount,
            Amount: amount
        );
    }
}
