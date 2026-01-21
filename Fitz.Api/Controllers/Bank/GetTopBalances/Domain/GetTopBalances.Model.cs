using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Bank.GetTopBalances.Domain;

public record GetTopBalancesModel(
    List<AccountEntity> Accounts)
{
    public static GetTopBalancesModel From(List<AccountEntity> accounts)
    {
        return new GetTopBalancesModel(Accounts: accounts);
    }
}
