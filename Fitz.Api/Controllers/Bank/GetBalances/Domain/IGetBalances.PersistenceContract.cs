using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Bank.GetBalances.Domain;

public interface IGetBalances
{
    Task<(List<AccountEntity> Accounts, int TotalCount)> GetBalancesAsync(int skip, int take, CancellationToken cancellationToken = default);
}
