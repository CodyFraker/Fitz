using DSharpPlus.Entities;

namespace Fitz.Api.Controllers.Lottery.Interactions;

public static class LotteryButtons
{
    public static DiscordButtonComponent CreateCancelButton(int uniqueId)
    {
        return new DiscordButtonComponent(
            DiscordButtonStyle.Danger,
            $"lottery_cancel_{uniqueId}",
            "Cancel",
            false);
    }

    public static DiscordButtonComponent CreateHelpButton(int uniqueId)
    {
        return new DiscordButtonComponent(
            DiscordButtonStyle.Secondary,
            $"lottery_help_{uniqueId}",
            "Help",
            false);
    }

    public static DiscordButtonComponent CreateBuyMaxTicketsButton(int uniqueId, bool disabled)
    {
        return new DiscordButtonComponent(
            DiscordButtonStyle.Success,
            $"lottery_max_tickets_{uniqueId}",
            "Buy Max Tickets",
            disabled);
    }

    public static DiscordButtonComponent CreateBuyXButton(int uniqueId, bool disabled)
    {
        return new DiscordButtonComponent(
            DiscordButtonStyle.Primary,
            $"lottery_buy_x_{uniqueId}",
            "Buy X",
            disabled);
    }
}
