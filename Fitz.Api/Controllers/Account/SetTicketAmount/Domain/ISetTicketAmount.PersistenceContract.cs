using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Account.SetTicketAmount.Domain;

public interface ISetTicketAmount
{
    Task<AccountEntity?> FindAccountByIdAsync(ulong userId, CancellationToken cancellationToken = default);
    Task UpdateAccountAsync(AccountEntity account, CancellationToken cancellationToken = default);
}
