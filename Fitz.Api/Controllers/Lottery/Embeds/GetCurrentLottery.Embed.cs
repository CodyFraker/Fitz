using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Api.Controllers.Lottery.GetCurrentLottery.Domain;
using Fitz.Database.Entities;
using Fitz.Variables.Emojis;

namespace Fitz.Api.Controllers.Lottery.Embeds;

public record GetCurrentLotteryEmbed
{
    private static readonly DiscordColor EmbedColor = new(52, 114, 53);

    public static DiscordEmbed LotteryCommandEmbed(
        DiscordClient dClient,
        GetCurrentLotteryResponse lottery,
        SettingsEntity settings,
        AccountEntity account,
        List<TicketEntity> userTickets,
        int remainingHours,
        int lastWinningTicket,
        int totalTickets,
        int totalParticipants)
    {
        DiscordEmbedBuilder embed = new()
        {
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                IconUrl = DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Ticket).Url,
                Text = $"Lottery #{lottery.Id} | Last Winning Ticket: {lastWinningTicket}",
            },
            Color = EmbedColor,
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Lottery).Url,
            },
            Title = "Lottery",
            Description =
                $"**__Your Entries__**: ```ansi\n\u001b[1;37m{userTickets.Count}\u001b[0;0m\n```\n" +
                $"**__Entries Available__**: ```{settings.MaxTickets - userTickets.Count}```\n"
        };

        embed.AddField("Info",
            $"{DiscordEmoji.FromName(dClient, ":beer:")} Beer Pool: `{lottery.Pool ?? 0}` \n" +
            $"{DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Ticket)} Total Tickets: `{totalTickets}`\n" +
            $"{DiscordEmoji.FromGuildEmote(dClient, AccountEmojis.Users)} Total Users: `{totalParticipants}`\n" +
            $"{DiscordEmoji.FromName(dClient, ":clock2:")} Time Left: `{remainingHours}` Hrs\n" +
            $"Ticket cost: `{settings.TicketCost}` beer\n", false);

        embed.AddField("**Starts**", $"```ansi\n\u001b[1;33m{lottery.StartDate}\u001b[0;0m\n```", false);
        embed.AddField("**Ends**", $"```ansi\n\u001b[1;31m{lottery.EndDate}\u001b[0;0m\n```", false);

        return embed.Build();
    }

    public static DiscordEmbed LotteryHelpEmbed(
        DiscordClient dClient,
        GetCurrentLotteryResponse lottery,
        SettingsEntity settings)
    {
        DiscordEmbedBuilder embed = new()
        {
            Title = "Lottery Help",
            Description = "A single ticket will grant you a chance of 1-1001. You can purchase up to 36 tickets. None of them will be a duplicate ticket.\n" +
                "If no one wins, the fridge will roll over into the next lottery, increasing the total beer.\n" +
                "Favorability is factored when more than one person wins.\n" +
                "I also play the lottery. I have no limit on the amount of tickets I can have.",
            Color = EmbedColor,
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Lottery).Url
            },
        };

        embed.AddField("Commands",
            $"`/lottery #` will buy a set amount of tickets. Providing 0 tickets will return this message again.\n" +
            $"\n" +
            $"`/lotteryinfo` will show you some basic information about the current drawing. The QR code will show you which tickets you have in this drawing.\n" +
            $"\n" +
            $"You can set your account to automatically play the lottery for by doing `/settings`.", false);

        return embed.Build();
    }
}
