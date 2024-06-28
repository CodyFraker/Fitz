using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Core.Commands.Attributes;
using Fitz.Core.Contexts;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank.Commands.Attributes;
using Fitz.Features.Bank.Models;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Bank.Commands
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public sealed class BankSlashCommands(BotContext db, BankService bankService, AccountService accountService) : ApplicationCommandModule
    {
        private readonly BotContext db = db;
        private readonly BankService bankService = bankService;
        private readonly AccountService AccountService = accountService;

        #region Fridge

        [SlashCommand("fridge", "Check how much beer you have in the fridge.")]
        [RequireAccount]
        public async Task Balance(InteractionContext ctx)
        {
            // Check to see if user has an account
            Account account = AccountService.FindAccount(ctx.User.Id);

            if (account == null)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You do not have an account. Please sign up using `/signup`.").AsEphemeral(true));
                return;
            }

            // Get latest transactions for the user
            List<Transaction> transactions = bankService.GetTransactions(account.Id);

            DiscordEmbedBuilder balanceEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(ctx.Client, LotteryEmojis.Ticket).Url,
                    Text = $"Bank",
                },
                Color = new DiscordColor(52, 114, 53),
                Timestamp = DateTime.UtcNow,
                Description = "Beer Balance"
            };

            string transactionsField = string.Empty;

            int length = 0;
            foreach (Transaction transaction in transactions)
            {
                length++;
                if (length >= 10)
                {
                    continue;
                }
                else
                {
                    transactionsField += $"{transaction.Amount} | {transaction.Reason} | {transaction.Timestamp.ToShortDateString()}\n";
                }
            }

            balanceEmbed.AddField("Beer", $"`{account.Beer}`", true);
            balanceEmbed.AddField("Lifetime Beer", $"`{account.LifetimeBeer}`", true);
            balanceEmbed.AddField("Transactions", $"{transactionsField}", false);

            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(balanceEmbed.Build()).AsEphemeral(true));

            // TODO: Make embed pretty.
        }

        #endregion Fridge

        #region Top Balances

        [SlashCommand("topbalances", "Get the top 10 balances for all users.")]
        [RequireAccount]
        public async Task Balances(InteractionContext ctx)
        {
            List<Account> accounts = bankService.GetTopBeerBalances();
            string table = accounts.Select(account => new
            {
                User = account.Username,
                Beer = account.Beer + " "
            }).ToMarkdownTable();

            DiscordEmbedBuilder balanceEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(ctx.Client, LotteryEmojis.Ticket).Url,
                    Text = $"Bank",
                },
                Color = new DiscordColor(52, 114, 53),
                Timestamp = DateTime.UtcNow,
                Description = $"```md\n{table}\n```",
            };

            //string usernames = string.Empty;
            //string beer = string.Empty;
            //foreach (Account account in accounts)
            //{
            //    usernames += $"{account.Username}\n";
            //    beer += $"`{account.Beer}`\n";
            //}

            //balanceEmbed.AddField($"**Username**", $"{usernames}", true);
            //balanceEmbed.AddField($"**Beer**", $"{beer}", true);
            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(balanceEmbed.Build()).AsEphemeral(true));
        }

        #endregion Top Balances

        #region Transactions

        [SlashCommand("transactions", "Get the last 10 transactions")]
        [RequireAccount]
        public async Task GetLastTransactions(InteractionContext ctx)
        {
            List<Transaction> transactions = bankService.GetTransactions(10);

            string table = transactions.Select(transaction => new
            {
                User = this.AccountService.FindAccount(transaction.Sender).Username,
                Beer = transaction.Amount,
                Type = transaction.Reason,
                Date = transaction.Timestamp.ToShortDateString()
            }).ToMarkdownTable();

            DiscordEmbedBuilder transactionEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(ctx.Client, LotteryEmojis.Ticket).Url,
                    Text = $"Bank",
                },
                Color = new DiscordColor(52, 114, 53),
                Timestamp = DateTime.UtcNow,
                Description = $"```md\n{table}\n```",
            };

            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(transactionEmbed.Build()).AsEphemeral(true));
        }

        #endregion Transactions

        #region Bank

        [SlashCommand("bank", "Add or remove money/beer from a user.")]
        [RequireBankTeller]
        public async Task BankAdmin(InteractionContext ctx, [Option("Action", "test1")] BankAction bankAction = BankAction.Add,
            [Option("Amount", "Amount to add/remove")] long amount = 0,
            [Option("User", "User to manage")] DiscordUser discordUser = null)
        {
            if (discordUser == null)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("No user was provided.").AsEphemeral(true));
            }
            Account account = AccountService.FindAccount(discordUser.Id);
            if (account == null)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("No user account was found for that user. Try signing them up instead.").AsEphemeral(true));
            }
            if (bankAction == BankAction.Add)
            {
                await bankService.AwardBonus(discordUser.Id, (int)amount);
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Added {amount} beer to {discordUser.Username}").AsEphemeral(true));
            }
            else if (bankAction == BankAction.Remove)
            {
                await bankService.DeductBeerFromUser(discordUser.Id, (int)amount, Reason.Donated);
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Remove {amount} beer from {discordUser.Username}").AsEphemeral(true));
            }
        }

        #endregion Bank
    }
}