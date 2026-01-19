namespace Fitz.Api.Controllers.Account.GetAccount.Domain;

public record GetAccountResponse(
    ulong Id,
    string? Username,
    int Beer,
    int LifetimeBeer,
    int SafeBalance,
    int Favorability,
    DateTime CreatedDate,
    bool SubscribeToLottery,
    int SubscribeTickets,
    bool Deactivated
)
{
    public static GetAccountResponse From(GetAccountModel model)
    {
        return new GetAccountResponse(
            Id: model.Id,
            Username: model.Username,
            Beer: model.Beer,
            LifetimeBeer: model.LifetimeBeer,
            SafeBalance: model.SafeBalance,
            Favorability: model.Favorability,
            CreatedDate: model.CreatedDate,
            SubscribeToLottery: model.SubscribeToLottery,
            SubscribeTickets: model.SubscribeTickets,
            Deactivated: model.Deactivated
        );
    }
}
