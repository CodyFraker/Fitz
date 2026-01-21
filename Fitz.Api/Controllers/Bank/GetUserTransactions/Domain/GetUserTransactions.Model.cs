using Transaction = Fitz.Database.Entities.Transaction;

namespace Fitz.Api.Controllers.Bank.GetUserTransactions.Domain;

public record GetUserTransactionsModel(
    List<Transaction> Transactions,
    int TotalCount)
{
    public static GetUserTransactionsModel From(List<Transaction> transactions, int totalCount)
    {
        return new GetUserTransactionsModel(Transactions: transactions, TotalCount: totalCount);
    }
}
