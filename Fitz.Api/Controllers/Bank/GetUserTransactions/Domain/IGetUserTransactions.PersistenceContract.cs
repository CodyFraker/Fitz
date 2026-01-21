using Transaction = Fitz.Database.Entities.Transaction;

namespace Fitz.Api.Controllers.Bank.GetUserTransactions.Domain;

public interface IGetUserTransactions
{
    Task<(List<Transaction> Transactions, int TotalCount)> GetUserTransactionsAsync(ulong userId, int skip, int take, CancellationToken cancellationToken = default);
}
