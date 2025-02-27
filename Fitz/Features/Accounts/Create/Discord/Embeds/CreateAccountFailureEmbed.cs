using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Variables.Emojis;
using System;

namespace Fitz.Features.Accounts.Create.Discord.Embeds
{
    public sealed class CreateAccountFailureEmbed
    {
        public DiscordEmbed BuildEmbed(DiscordClient dClient, DiscordUser user)
        {
            DiscordEmbedBuilder CreateAccountFailedEmbed = new()
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(dClient, AccountEmojis.Users).Url,
                    Text = $"Create Account Failed",
                },
                Color = new DiscordColor(255, 0, 0),
                Timestamp = DateTime.UtcNow,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = user.AvatarUrl,
                },
                Description = "Failed to create account. \n" +
                "Could be a fluke or Cody just sucks at code.\n" +
                "Please try again later."
            };

            return CreateAccountFailedEmbed.Build();
        }
    }
}