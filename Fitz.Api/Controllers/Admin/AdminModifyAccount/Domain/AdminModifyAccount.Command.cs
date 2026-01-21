using Fitz.Api.Controllers.Admin.AdminModifyAccount.Http;

namespace Fitz.Api.Controllers.Admin.AdminModifyAccount.Domain;

public record AdminModifyAccountCommand(
    ulong UserId,
    int? Beer,
    int? LifetimeBeer,
    int? SafeBalance,
    int? Favorability,
    bool? SubscribeToLottery,
    int? SubscribeTickets,
    bool? Deactivated)
{
    public static AdminModifyAccountCommand From(ulong userId, AdminModifyAccountRequestDto request)
    {
        return new AdminModifyAccountCommand(
            UserId: userId,
            Beer: request.Beer,
            LifetimeBeer: request.LifetimeBeer,
            SafeBalance: request.SafeBalance,
            Favorability: request.Favorability,
            SubscribeToLottery: request.SubscribeToLottery,
            SubscribeTickets: request.SubscribeTickets,
            Deactivated: request.Deactivated
        );
    }
}
