using Transaction = Fitz.Database.Entities.Transaction;

namespace Fitz.Api.Controllers.Bank.GetTransactions.Domain;

public record GetTransactionsResponse(
    List<Transaction> Transactions)
{
    public static GetTransactionsResponse From(GetTransactionsModel model)
    {
        return new GetTransactionsResponse(Transactions: model.Transactions);
    }
}
