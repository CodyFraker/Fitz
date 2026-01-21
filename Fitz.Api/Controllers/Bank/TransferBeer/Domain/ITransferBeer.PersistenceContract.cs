using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Bank.TransferBeer.Domain;

public interface ITransferBeer
{
    Task<AccountEntity?> FindAccountByIdAsync(ulong userId, CancellationToken cancellationToken = default);
    Task UpdateAccountAsync(AccountEntity account, CancellationToken cancellationToken = default);
    Task LogTransactionAsync(ulong senderId, ulong recipientId, int amount, Reason reason, CancellationToken cancellationToken = default);
}
