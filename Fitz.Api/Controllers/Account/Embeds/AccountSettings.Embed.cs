using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Api.Controllers.Account.GetAccountSettings.Domain;
using Fitz.Variables.Emojis;

namespace Fitz.Api.Controllers.Account.Embeds;

public static class AccountSettingsEmbed
{
    private static readonly DiscordColor EmbedColor = new(52, 114, 53);

    public static DiscordEmbed FromGetAccountSettings(DiscordClient discordClient, DiscordUser discordUser, GetAccountSettingsModel model)
    {
        DiscordEmbedBuilder settingsEmbed = new()
        {
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                IconUrl = DiscordEmoji.FromGuildEmote(discordClient, AccountEmojis.Edit).Url,
                Text = $"Account Settings | ID: {model.Id}",
            },
            Color = EmbedColor,
            Timestamp = DateTime.UtcNow,
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = discordUser.AvatarUrl,
            },
            Description = $"Change your account settings using the buttons below.\n"
        };

        if (model.SubscribeToLottery)
        {
            settingsEmbed.AddField($"{DiscordEmoji.FromGuildEmote(discordClient, LotteryEmojis.Lottery)} __**Lottery Subscription**__: `Active` {DiscordEmoji.FromName(discordClient, ":white_check_mark:", true)}", $"If active, Fitz will buy tickets for you each lottery.");
        }
        else
        {
            settingsEmbed.AddField($"{DiscordEmoji.FromGuildEmote(discordClient, LotteryEmojis.Lottery)} __**Lottery Subscription**__: `Inactive` {DiscordEmoji.FromName(discordClient, ":x:", true)}", $"If active, Fitz will buy tickets for you each lottery.");
        }

        settingsEmbed.AddField($"{DiscordEmoji.FromName(discordClient, ":beer:", true)} __**Safe Balance**__: {model.SafeBalance}", $"The amount of money you want before you stop auto-entering the lottery.", false);
        settingsEmbed.AddField($"{DiscordEmoji.FromGuildEmote(discordClient, LotteryEmojis.Ticket)} __**Tickets**__: {model.SubscribeTickets}", $"The number of tickets you want to buy each lottery.", false);

        return settingsEmbed.Build();
    }
}
