using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Features.Bank.GetBalance.Domain;
using Fitz.Features.Bank.Models;
using Fitz.Variables.Emojis;

namespace Fitz.Features.Bank.GetBalance.Discord
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public class GetBalanceAdapter : ApplicationCommandModule
    {
        private readonly GetBalanceService _balanceService;

        public GetBalanceAdapter(GetBalanceService balanceService)
        {
            _balanceService = balanceService ?? throw new ArgumentNullException(nameof(balanceService));
        }

        [SlashCommand("fridge", "Check how much beer you have in the fridge")]
        public async Task GetBalance(InteractionContext ctx)
        {
            try
            {
                var command = new GetBalanceCommand(ctx.User.Id, true, 10);
                var (balance, transactions) = await _balanceService.GetBalanceAsync(command);

                var embed = new DiscordEmbedBuilder()
                    .WithTitle("Beer Balance")
                    .WithColor(new DiscordColor(52, 114, 53))
                    .WithTimestamp(DateTime.UtcNow)
                    .WithFooter("Bank", DiscordEmoji.FromGuildEmote(ctx.Client, LotteryEmojis.Ticket).Url);

                embed.AddField("Beer", $"`{balance}`", true);
                
                // Add transactions if available
                if (transactions.Any())
                {
                    string transactionsField = string.Empty;
                    int count = 0;
                    
                    foreach (var transaction in transactions)
                    {
                        count++;
                        if (count > 10) break;
                        
                        transactionsField += $"{transaction.Amount} | {transaction.Reason} | {transaction.Timestamp.ToShortDateString()}\n";
                    }
                    
                    embed.AddField("Transactions", transactionsField, false);
                }

                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .AddEmbed(embed.Build())
                        .AsEphemeral(true));
            }
            catch (Exception ex)
            {
                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"Error retrieving balance: {ex.Message}")
                        .AsEphemeral(true));
            }
        }

        [SlashCommand("topbalances", "Get the top 10 balances for all users")]
        public async Task GetTopBalances(InteractionContext ctx)
        {
            try
            {
                var topBalances = await _balanceService.GetTopBalancesAsync(10);
                
                var embed = new DiscordEmbedBuilder()
                    .WithTitle("Top Beer Balances")
                    .WithColor(new DiscordColor(52, 114, 53))
                    .WithTimestamp(DateTime.UtcNow)
                    .WithFooter("Bank", DiscordEmoji.FromGuildEmote(ctx.Client, LotteryEmojis.Ticket).Url);

                string balancesText = string.Empty;
                int rank = 1;
                
                foreach (var (userId, username, balance) in topBalances)
                {
                    balancesText += $"{rank}. {username}: {balance} beer\n";
                    rank++;
                }
                
                embed.WithDescription(balancesText);

                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .AddEmbed(embed.Build())
                        .AsEphemeral(true));
            }
            catch (Exception ex)
            {
                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"Error retrieving top balances: {ex.Message}")
                        .AsEphemeral(true));
            }
        }
    }
}
