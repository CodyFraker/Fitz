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
using System.Security.Principal;
using System.Threading.Tasks;

namespace Fitz.Features.Bank.Commands
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    internal class BankSlashCommands : ApplicationCommandModule
    {
        private readonly BotContext db;
        private readonly BankService bankService;
        private AccountService AccountService;

        public BankSlashCommands(BotContext db, BankService bankService, AccountService accountService)
        {
            this.db = db;
            this.bankService = bankService;
            AccountService = accountService;
        }

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

            balanceEmbed.AddField("Beer", $"`{account.Beer}`", true);
            balanceEmbed.AddField("Lifetime Beer", $"`{account.LifetimeBeer}`", true);

            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(balanceEmbed.Build()).AsEphemeral(true));

            // TODO: Add transaction history to embed. Make embed pretty.
        }

        [SlashCommand("topbalances", "Get the top 10 balances for all users.")]
        [RequireAccount]
        public async Task Balances(InteractionContext ctx)
        {
            List<Account> accounts = bankService.GetTopBeerBalances();

            DiscordEmbedBuilder balanceEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(ctx.Client, LotteryEmojis.Ticket).Url,
                    Text = $"Bank",
                },
                Color = new DiscordColor(52, 114, 53),
                Timestamp = DateTime.UtcNow,
                Description = "Top Beer Balances"
            };

            string usernames = string.Empty;
            string beer = string.Empty;
            foreach (Account account in accounts)
            {
                usernames += $"{account.Username}\n";
                beer += $"`{account.Beer}`\n";
            }

            balanceEmbed.AddField($"**Username**", $"{usernames}", true);
            balanceEmbed.AddField($"**Beer**", $"{beer}", true);
            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(balanceEmbed.Build()).AsEphemeral(true));
        }

        [SlashCommand("transactions", "Get the last 10 transactions")]
        [RequireAccount]
        public async Task GetLastTransactions(InteractionContext ctx)
        {
            List<Transaction> transactions = bankService.GetTransactions(10);

            DiscordEmbedBuilder transactionEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(ctx.Client, LotteryEmojis.Ticket).Url,
                    Text = $"Bank",
                },
                Color = new DiscordColor(52, 114, 53),
                Timestamp = DateTime.UtcNow,
                Description = "Last 10 Transactions"
            };

            string usernames = string.Empty;
            string beer = string.Empty;
            string transactionType = string.Empty;
            string transactionDate = string.Empty;
            foreach (Transaction transaction in transactions)
            {
                usernames += $"{transaction.Recipient}\n";
                beer += $"`{transaction.Amount}`\n";
                transactionType += $"{transaction.Reason}\n";
                transactionDate += $"{transaction.Timestamp}\n";
            }

            transactionEmbed.AddField($"**Username**", $"{usernames}", true);
            transactionEmbed.AddField($"**Beer**", $"{beer}", true);
            transactionEmbed.AddField($"**Type**", $"{transactionType}", true);
            transactionEmbed.AddField($"**Date**", $"{transactionDate}", true);
            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(transactionEmbed.Build()).AsEphemeral(true));
        }

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
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Remove action.{amount}"));
            }
        }

        public enum BankAction
        {
            [ChoiceName("Add")]
            Add,

            [ChoiceName("Remove")]
            Remove
        }
    }
}