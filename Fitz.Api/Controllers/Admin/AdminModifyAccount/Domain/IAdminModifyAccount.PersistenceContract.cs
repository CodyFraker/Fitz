using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Admin.AdminModifyAccount.Domain;

public interface IAdminModifyAccount
{
    Task<AccountEntity?> FindAccountByIdAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<AccountEntity> UpdateAccountAsync(AccountEntity account, CancellationToken cancellationToken = default);
    Task<AccountEntity?> GetAccountAfterUpdateAsync(ulong userId, CancellationToken cancellationToken = default);
}
