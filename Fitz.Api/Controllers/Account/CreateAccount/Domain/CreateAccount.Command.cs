using DSharpPlus.SlashCommands;

namespace Fitz.Api.Controllers.Account.CreateAccount.Domain;

public record CreateAccountCommand(
    ulong Id,
    string Username,
    ulong GuildId,
    DateTime CreatedOn,
    DateTime LastSeenDate,
    bool SubscribedToLottery,
    int SubscribeTickets,
    bool Deactivated
    )
{
    public static CreateAccountCommand FromInteractionContext(InteractionContext interactionContext)
    {
        return new CreateAccountCommand(
            Id: interactionContext.User.Id,
            Username: interactionContext.User.Username,
            GuildId: interactionContext.Guild.Id,
            CreatedOn: DateTime.UtcNow,
            LastSeenDate: DateTime.UtcNow,
            SubscribedToLottery: false,
            SubscribeTickets: 1,
            Deactivated: false
            );
    }
}
