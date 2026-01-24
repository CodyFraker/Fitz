using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Api.Controllers.Bank.GetBalanceWithTransactions.Domain;
using Fitz.Variables.Emojis;
using Transaction = Fitz.Database.Entities.Transaction;

namespace Fitz.Api.Controllers.Bank.GetBalanceWithTransactions.Embeds;

public static class BalanceEmbed
{
    private static readonly DiscordColor EmbedColor = new(52, 114, 53);

    public static DiscordEmbed FromGetBalanceWithTransactions(DiscordClient discordClient, GetBalanceWithTransactionsModel model)
    {
        DiscordEmbedBuilder balanceEmbed = new()
        {
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                IconUrl = DiscordEmoji.FromGuildEmote(discordClient, LotteryEmojis.Ticket).Url,
                Text = $"Bank",
            },
            Color = EmbedColor,
            Timestamp = DateTime.UtcNow,
            Description = "Beer Balance"
        };

        string transactionsField = string.Empty;

        int length = 0;
        foreach (Transaction transaction in model.Transactions)
        {
            length++;
            if (length > 10)
            {
                break;
            }
            
            transactionsField += $"{transaction.Amount} | {transaction.Reason} | {transaction.Timestamp.ToShortDateString()}\n";
        }

        if (string.IsNullOrWhiteSpace(transactionsField))
        {
            transactionsField = "No transactions yet";
        }

        balanceEmbed.AddField("Beer", $"`{model.Account.Beer}`", true);
        balanceEmbed.AddField("Lifetime Beer", $"`{model.Account.LifetimeBeer}`", true);
        balanceEmbed.AddField("Transactions", transactionsField, false);

        return balanceEmbed.Build();
    }
}
