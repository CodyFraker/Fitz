using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Bank.GetBalance.Domain;

public interface IGetBalance
{
    Task<AccountEntity?> FindAccountByIdAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<AccountEntity> CreateAccountAsync(ulong userId, string username, CancellationToken cancellationToken = default);
}
