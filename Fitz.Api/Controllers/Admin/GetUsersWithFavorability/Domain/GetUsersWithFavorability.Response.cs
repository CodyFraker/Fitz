using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Admin.GetUsersWithFavorability.Domain;

public record GetUsersWithFavorabilityResponse(
    List<AccountEntity> Accounts,
    int TotalCount,
    int BotBeer)
{
    public static GetUsersWithFavorabilityResponse From(GetUsersWithFavorabilityModel model)
    {
        return new GetUsersWithFavorabilityResponse(Accounts: model.Accounts, TotalCount: model.TotalCount, BotBeer: model.BotBeer);
    }
}
