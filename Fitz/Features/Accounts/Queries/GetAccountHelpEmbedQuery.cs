using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Variables.Emojis;
using System;

namespace Fitz.Features.Accounts.Queries
{
    public class GetAccountHelpEmbedQuery
    {
        public DiscordEmbed Execute(DiscordClient dClient)
        {
            var accountHelpEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(dClient, AccountEmojis.Users).Url,
                    Text = $"Account Command Help",
                },
                Color = new DiscordColor(52, 114, 53),
                Timestamp = DateTime.UtcNow,
                Title = "Account Command Help",
                Description = "**Commands**\n" +
                $"`/signup`: Create an account with me. Everyone needs one.\n" +
                $"`/settings`: Edit your account settings.\n" +
                $"`/account`: View your account details\n"
            };

            return accountHelpEmbed.Build();
        }
    }
}
