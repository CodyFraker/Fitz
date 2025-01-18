using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Variables.Emojis;
using System;

namespace Fitz.Features.AccountsRework.Create.Discord.Embeds
{
    public sealed class CreateAccountConflictedEmbed
    {
        public DiscordEmbed BuildEmbed(DiscordClient dClient, DiscordUser user)
        {
            DiscordEmbedBuilder CreateAccountConflictEmbed = new()
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(dClient, AccountEmojis.Users).Url,
                    Text = $"Account Information",
                },
                Color = new DiscordColor(255, 0, 0),
                Timestamp = DateTime.UtcNow,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = user.AvatarUrl,
                },
                // TODO: Fill out the description with the user's account information.
                Description = "You already have an account. \n" +
                "Try using `/profile` to see your account details.\n" +
                "Otherwise, wait until I get around to filling out this card with your existing account info. Idiot."
            };

            return CreateAccountConflictEmbed.Build();
        }
    }
}