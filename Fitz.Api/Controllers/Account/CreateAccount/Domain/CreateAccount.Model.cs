namespace Fitz.Api.Controllers.Account.CreateAccount.Domain;

public record CreateAccountModel(
    ulong Id,
    string Username,
    int Beer,
    int LifetimeBeer,
    int SafeBalance,
    int Favorability,
    DateTime CreatedOn,
    DateTime LastSeenDate,
    DateTime LastActivityDate,
    bool SubscribedToLottery,
    int SubscribeTickets,
    bool Deactivated)
{
    public static CreateAccountModel From(CreateAccountCommand createAccountCommand)
    {
        return new CreateAccountModel(
            Id: createAccountCommand.Id,
            Username: createAccountCommand.Username,
            Beer: 0,
            LifetimeBeer: 0,
            SafeBalance: 128,
            Favorability: 50,
            CreatedOn: createAccountCommand.CreatedOn,
            LastSeenDate: createAccountCommand.LastSeenDate,
            LastActivityDate: createAccountCommand.CreatedOn,
            SubscribedToLottery: createAccountCommand.SubscribedToLottery,
            SubscribeTickets: createAccountCommand.SubscribeTickets,
            Deactivated: createAccountCommand.Deactivated
        );
    }
}
