using Transaction = Fitz.Database.Entities.Transaction;

namespace Fitz.Api.Controllers.Bank.GetTransactions.Domain;

public record GetTransactionsModel(
    List<Transaction> Transactions)
{
    public static GetTransactionsModel From(List<Transaction> transactions)
    {
        return new GetTransactionsModel(Transactions: transactions);
    }
}
