using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Account.GetAccount.Domain;

public interface IGetAccount
{
    Task<AccountEntity?> FindByIdAsync(ulong id, CancellationToken cancellationToken = default);
}
