using DSharpPlus.SlashCommands;

namespace Fitz.Api.Controllers.Account.GiveBeer.Domain;

public record GiveBeerCommand(ulong UserId, int Amount)
{
    public static GiveBeerCommand FromInteractionContext(InteractionContext ctx, double amount)
    {
        return new GiveBeerCommand(UserId: ctx.User.Id, Amount: (int)amount);
    }
}
