using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Account.DeactivateAccount.Domain;

public interface IDeactivateAccount
{
    Task<AccountEntity?> FindAccountByIdAsync(ulong userId, CancellationToken cancellationToken = default);
    Task UpdateAccountAsync(AccountEntity account, CancellationToken cancellationToken = default);
}
