using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Api.Controllers.Lottery.GetLotteryStatistics.Domain;
using Fitz.Variables.Emojis;

namespace Fitz.Api.Controllers.Lottery.Embeds;

public record GetLotteryStatisticsEmbed
{
    private static readonly DiscordColor EmbedColor = new(52, 114, 53);

    public static DiscordEmbed LotteryStatisticsEmbed(
        DiscordClient dClient,
        GetLotteryStatisticsResponse response)
    {
        DiscordEmbedBuilder embed = new()
        {
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                IconUrl = DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Ticket).Url,
                Text = "All Time Statistics",
            },
            Color = EmbedColor,
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Lottery).Url,
            },
            Title = "Lottery Statistics",
            Timestamp = DateTime.UtcNow
        };

        if (response.DataPoints.Count == 0)
        {
            embed.Description = "No statistics available.";
            return embed.Build();
        }

        var totalLotteries = response.DataPoints.Count;
        var totalPrizePool = response.DataPoints.Sum(p => p.PrizePool);
        var averagePrizePool = totalPrizePool / (double)totalLotteries;
        var totalTickets = response.DataPoints.Sum(p => p.TotalTickets);
        var averageTickets = totalTickets / (double)totalLotteries;
        var largestPrizePool = response.DataPoints.Max(p => p.PrizePool);
        var mostTickets = response.DataPoints.Max(p => p.TotalTickets);

        var descriptionParts = new List<string>
        {
            $"**Total Lotteries**: `{totalLotteries:N0}`",
            $"**Total Prize Pool**: `{totalPrizePool:N0}` beer",
            $"**Average Prize Pool**: `{averagePrizePool:F2}` beer",
            $"**Total Tickets**: `{totalTickets:N0}`",
            $"**Average Tickets**: `{averageTickets:F2}` per lottery",
            $"**Largest Prize Pool**: `{largestPrizePool:N0}` beer",
            $"**Most Tickets**: `{mostTickets:N0}`"
        };

        if (response.AverageTicketsPerWinner.HasValue)
        {
            descriptionParts.Add($"**Average Tickets Per Winner**: `{response.AverageTicketsPerWinner.Value:F2}`");
        }
        else
        {
            descriptionParts.Add("**Average Tickets Per Winner**: `N/A`");
        }

        embed.Description = string.Join("\n", descriptionParts);

        return embed.Build();
    }
}
