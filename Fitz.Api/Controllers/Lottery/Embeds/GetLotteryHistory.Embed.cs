using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Api.Controllers.Lottery.GetLotteryHistory.Domain;
using Fitz.Variables.Emojis;

namespace Fitz.Api.Controllers.Lottery.Embeds;

public record GetLotteryHistoryEmbed
{
    private static readonly DiscordColor EmbedColor = new(52, 114, 53);

    public static DiscordEmbed LotteryHistoryEmbed(
        DiscordClient dClient,
        GetLotteryHistoryResponse response)
    {
        DiscordEmbedBuilder embed = new()
        {
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                IconUrl = DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Ticket).Url,
                Text = "Past 5 Lotteries",
            },
            Color = EmbedColor,
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Lottery).Url,
            },
            Title = "Lottery History",
            Timestamp = DateTime.UtcNow
        };

        if (response.Lotteries.Count == 0)
        {
            embed.Description = "No lottery history available.";
            return embed.Build();
        }

        var descriptionParts = new List<string>();

        foreach (var lottery in response.Lotteries)
        {
            var dateStr = lottery.EndDate.ToString("MM/dd/yyyy");
            var winningTicketStr = lottery.WinningTicket?.ToString() ?? "N/A";
            
            string winnerStr;
            string payoutStr;

            if (lottery.Winners.Count == 0)
            {
                winnerStr = "No Winner";
                payoutStr = "0";
            }
            else
            {
                var winnerNames = lottery.Winners
                    .Select(w => w.Username ?? $"User {w.AccountId}")
                    .ToList();
                winnerStr = string.Join(", ", winnerNames);
                payoutStr = lottery.Winners.First().Payout.ToString();
            }

            descriptionParts.Add(
                $"**Lottery #{lottery.Id}** - {dateStr}\n" +
                $"Winning Ticket: `{winningTicketStr}` | Winner: {winnerStr} | Payout: `{payoutStr}` beer\n"
            );
        }

        embed.Description = string.Join("\n", descriptionParts);

        return embed.Build();
    }
}
