using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Features.Accounts.Models;
using Fitz.Variables.Emojis;
using System;

namespace Fitz.Features.Accounts.Create.Discord.Embeds
{
    public sealed class CreatedAccountSuccessEmbed
    {
        public DiscordEmbed BuildEmbed(DiscordClient dClient, DiscordUser user, Account account)
        {
            DiscordEmbedBuilder CreateAccountEmbed = new()
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(dClient, AccountEmojis.Users).Url,
                    Text = $"Account Created",
                },
                Color = new DiscordColor(52, 114, 53),
                Timestamp = DateTime.UtcNow,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = user.AvatarUrl,
                },
                Description =
                $"Edit your account settings using `/settings`\n\n" +
                $"**Beer**: `{account.Beer}`\n" +
                $"**Lifetime Beer**: `{account.LifetimeBeer}`\n" +
                $"**Favorability**: `{account.Favorability}%`\n" +
                $"**Lottery Subscription**: `{account.subscribeToLottery}`\n" +
                $"**Safe Balance**: `{account.safeBalance}`"
            };

            return CreateAccountEmbed.Build();
        }
    }
}