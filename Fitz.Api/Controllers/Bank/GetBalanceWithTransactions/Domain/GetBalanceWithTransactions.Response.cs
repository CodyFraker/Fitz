using Fitz.Database.Entities;
using Transaction = Fitz.Database.Entities.Transaction;

namespace Fitz.Api.Controllers.Bank.GetBalanceWithTransactions.Domain;

public record GetBalanceWithTransactionsResponse(
    AccountEntity Account,
    List<Transaction> Transactions)
{
    public static GetBalanceWithTransactionsResponse From(GetBalanceWithTransactionsModel model)
    {
        return new GetBalanceWithTransactionsResponse(
            Account: model.Account,
            Transactions: model.Transactions
        );
    }
}
