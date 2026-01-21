using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Admin.GetUsersWithFavorability.Domain;

public record GetUsersWithFavorabilityModel(
    List<AccountEntity> Accounts,
    int TotalCount,
    int BotBeer)
{
    public static GetUsersWithFavorabilityModel From(List<AccountEntity> accounts, int totalCount, int botBeer)
    {
        return new GetUsersWithFavorabilityModel(Accounts: accounts, TotalCount: totalCount, BotBeer: botBeer);
    }
}
