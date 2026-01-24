using Fitz.Api.Controllers.Bank.GetBalance.Domain;
using Fitz.Api.Controllers.Bank.GetUserTransactions.Domain;
using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Bank.GetBalanceWithTransactions.Domain;

public record GetBalanceWithTransactionsModel(
    AccountEntity Account,
    List<Transaction> Transactions)
{
    public static GetBalanceWithTransactionsModel From(GetBalanceModel balanceModel, GetUserTransactionsModel transactionsModel)
    {
        return new GetBalanceWithTransactionsModel(
            Account: balanceModel.Account,
            Transactions: transactionsModel.Transactions
        );
    }
}
