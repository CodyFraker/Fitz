using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Bank.GetBalances.Domain;

public record GetBalancesModel(
    List<AccountEntity> Accounts,
    int TotalCount)
{
    public static GetBalancesModel From(List<AccountEntity> accounts, int totalCount)
    {
        return new GetBalancesModel(Accounts: accounts, TotalCount: totalCount);
    }
}
