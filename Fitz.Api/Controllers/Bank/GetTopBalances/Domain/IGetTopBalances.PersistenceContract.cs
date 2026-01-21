using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Bank.GetTopBalances.Domain;

public interface IGetTopBalances
{
    Task<List<AccountEntity>> GetTopBalancesAsync(int limit, CancellationToken cancellationToken = default);
}
