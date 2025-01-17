using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitz.Core.Commands
{
    [SlashCommandGroup("leaderboard", "View the leaderboard of of a particular thing.")]
    public sealed class LeaderboardSlashCommands(BankService bankService) : ApplicationCommandModule
    {
        private readonly BankService bankService = bankService;

        [SlashCommand("beer", "View the leaderboard of the richest users.")]
        public async Task beerLeaderboard(InteractionContext ctx)
        {
            List<Account> accounts = bankService.GetTopBeerBalances();

            string topBalances = string.Empty;
            foreach (var account in accounts)
            {
                topBalances += $"{account.Username} - {account.Beer}\n";
            }

            DiscordEmbedBuilder lotteryEmbed = new()
            {
                Color = new DiscordColor(52, 114, 53),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = DiscordEmoji.FromGuildEmote(ctx.Client, LotteryEmojis.Lottery).Url,
                },
                Title = $"Top Beer Balances",
                Timestamp = DateTime.UtcNow,
                Description = $"{topBalances}",
            };

            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                               new DiscordInteractionResponseBuilder()
                                              .AddEmbed(lotteryEmbed.Build())
                                                             .AsEphemeral(true));
        }
    }
}