using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Bank.GetTopBalances.Domain;

public record GetTopBalancesResponse(
    List<AccountEntity> Accounts)
{
    public static GetTopBalancesResponse From(GetTopBalancesModel model)
    {
        return new GetTopBalancesResponse(Accounts: model.Accounts);
    }
}
