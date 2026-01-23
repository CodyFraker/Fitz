using DSharpPlus.SlashCommands;
using Fitz.Api.Controllers.Lottery.BuyTickets.Http;

namespace Fitz.Api.Controllers.Lottery.BuyTickets.Domain;

public record BuyTicketsCommand(ulong UserId, int Amount)
{
    public static BuyTicketsCommand From(BuyTicketsRequestDto request)
    {
        return new BuyTicketsCommand(UserId: request.UserId, Amount: request.Amount);
    }

    public static BuyTicketsCommand FromInteractionContext(InteractionContext ctx, int amount)
    {
        return new BuyTicketsCommand(UserId: ctx.User.Id, Amount: amount);
    }
}
