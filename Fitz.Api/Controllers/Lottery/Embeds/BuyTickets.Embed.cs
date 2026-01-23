using DSharpPlus.Entities;
using Fitz.Api.Controllers.Lottery.BuyTickets.Domain;

namespace Fitz.Api.Controllers.Lottery.Embeds;

public record BuyTicketsEmbed
{
    private static readonly DiscordColor EmbedColor = new(52, 114, 53);

    public static DiscordEmbed FromBuyTickets(BuyTicketsResponse response)
    {
        string ticketNumbers = string.Empty;
        foreach (var ticket in response.Tickets)
        {
            ticketNumbers += $"{ticket.Number}\n";
        }

        DiscordEmbedBuilder embed = new()
        {
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = "Ticket Purchase Successful",
            },
            Color = EmbedColor,
            Title = $"Successfully purchased {response.TicketsPurchased} ticket(s)",
            Description = $"**Total Cost**: `{response.TotalCost}` beer\n" +
                         $"**Tickets Purchased**: `{response.TicketsPurchased}`\n\n" +
                         $"**Your Ticket Numbers**:\n```\n{ticketNumbers}```",
            Timestamp = DateTime.UtcNow
        };

        return embed.Build();
    }
}
