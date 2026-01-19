using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Account.GetAccount.Domain;

public record GetAccountModel(
    ulong Id,
    string? Username,
    int Beer,
    int LifetimeBeer,
    int SafeBalance,
    int Favorability,
    DateTime CreatedDate,
    DateTime LastSeenDate,
    DateTime LastActivityDate,
    bool SubscribeToLottery,
    int SubscribeTickets,
    bool Deactivated)
{
    public static GetAccountModel From(AccountEntity entity)
    {
        return new GetAccountModel(
            Id: entity.Id,
            Username: entity.Username,
            Beer: entity.Beer,
            LifetimeBeer: entity.LifetimeBeer,
            SafeBalance: entity.safeBalance,
            Favorability: entity.Favorability,
            CreatedDate: entity.CreatedDate,
            LastSeenDate: entity.LastSeenDate,
            LastActivityDate: entity.LastActivityDate,
            SubscribeToLottery: entity.subscribeToLottery,
            SubscribeTickets: entity.SubscribeTickets,
            Deactivated: entity.Deactivated
        );
    }
}
