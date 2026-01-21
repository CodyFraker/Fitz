using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Account.SetLotterySubscribe.Domain;

public interface ISetLotterySubscribe
{
    Task<AccountEntity?> FindAccountByIdAsync(ulong userId, CancellationToken cancellationToken = default);
    Task UpdateAccountAsync(AccountEntity account, CancellationToken cancellationToken = default);
}
