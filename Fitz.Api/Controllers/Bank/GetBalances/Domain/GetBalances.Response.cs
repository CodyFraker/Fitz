using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Bank.GetBalances.Domain;

public record GetBalancesResponse(
    List<AccountEntity> Accounts,
    int TotalCount)
{
    public static GetBalancesResponse From(GetBalancesModel model)
    {
        return new GetBalancesResponse(
            Accounts: model.Accounts,
            TotalCount: model.TotalCount
        );
    }
}
