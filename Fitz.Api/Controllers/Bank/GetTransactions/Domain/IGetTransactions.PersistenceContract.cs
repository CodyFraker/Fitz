using Transaction = Fitz.Database.Entities.Transaction;

namespace Fitz.Api.Controllers.Bank.GetTransactions.Domain;

public interface IGetTransactions
{
    Task<List<Transaction>> GetTransactionsAsync(int take, CancellationToken cancellationToken = default);
}
