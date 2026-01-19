namespace Fitz.Api.Controllers.Account.CreateAccount.Domain;

public record CreateAccountResponse(
    ulong Id,
    string Username,
    int Beer,
    int LifetimeBeer,
    int SafeBalance,
    int Favorability,
    DateTime CreatedOn,
    DateTime LastSeenDate,
    bool SubscribedToLottery,
    int SubscribeTickets,
    bool Deactivated
    )
{
    public static CreateAccountResponse From(CreateAccountModel model)
    {
        return new CreateAccountResponse(
            Id: model.Id,
            Username: model.Username,
            Beer: model.Beer,
            LifetimeBeer: model.LifetimeBeer,
            SafeBalance: model.SafeBalance,
            Favorability: model.Favorability,
            CreatedOn: model.CreatedOn,
            LastSeenDate: model.LastSeenDate,
            SubscribedToLottery: model.SubscribedToLottery,
            SubscribeTickets: model.SubscribeTickets,
            Deactivated: model.Deactivated
        );
    }
}