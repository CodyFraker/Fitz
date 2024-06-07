using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Variables.Emojis;
using System;

namespace Fitz.Features.HappyHour
{
    public sealed class HappyHourService
    {
        public DiscordEmbed HappyHourHelpEmbed(DiscordClient dClient)
        {
            DiscordEmbedBuilder happyHourHelpEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(dClient, AccountEmojis.Users).Url,
                    Text = $"Happy Hour Help",
                },
                Color = new DiscordColor(52, 114, 53),
                Timestamp = DateTime.UtcNow,
                Title = "Account Command Help",
                Description = "Happy hour runs from 8PM - 11:59PM EST\n" +
                "If you're in the voice channel and have an account during this time, you will gain beer.\n" +
                "The amount of beer given out during this time can change at any given time. Even during happy hour."
            };

            return happyHourHelpEmbed.Build();
        }
    }
}