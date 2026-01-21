using Transaction = Fitz.Database.Entities.Transaction;

namespace Fitz.Api.Controllers.Bank.GetUserTransactions.Domain;

public record GetUserTransactionsResponse(
    List<Transaction> Transactions,
    int TotalCount)
{
    public static GetUserTransactionsResponse From(GetUserTransactionsModel model)
    {
        return new GetUserTransactionsResponse(Transactions: model.Transactions, TotalCount: model.TotalCount);
    }
}
