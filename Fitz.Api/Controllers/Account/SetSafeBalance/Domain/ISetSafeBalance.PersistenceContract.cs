using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Account.SetSafeBalance.Domain;

public interface ISetSafeBalance
{
    Task<AccountEntity?> FindAccountByIdAsync(ulong userId, CancellationToken cancellationToken = default);
    Task UpdateAccountAsync(AccountEntity account, CancellationToken cancellationToken = default);
}
