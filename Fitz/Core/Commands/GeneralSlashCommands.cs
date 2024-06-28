using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Core.Commands.Attributes;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using System;
using System.Threading.Tasks;

namespace Fitz.Core.Commands
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public sealed class GeneralSlashCommands(BankService bankService, AccountService accountService) : ApplicationCommandModule
    {
        private readonly BankService bankService = bankService;
        private readonly AccountService accountService = accountService;

        [SlashCommand("beer", "Give a beer to Fitz")]
        [RequireAccount]
        public async Task GiveBeer(InteractionContext ctx, [Option("Beer", "How much beer do you want to give Fitz?", false)] double amount = 0)
        {
            Account account = accountService.FindAccount(ctx.User.Id);

            if (account == null)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You don't have an account yet!").AsEphemeral(true));
                return;
            }

            if (account.Beer < amount)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You don't have enough money to give that much beer!").AsEphemeral(true));
                return;
            }

            // Percentage that represents the amount of beer the user wants to give to Fitz relative to their current total amount.
            double percentageOfBeer = amount / account.Beer * 100;

            double newFavorability = 0;

            if (account.Favorability <= 5)
            {
                newFavorability = account.Favorability + (percentageOfBeer * .2);
            }
            if (account.Favorability <= 20 && account.Favorability >= 6)
            {
                newFavorability = account.Favorability + (percentageOfBeer * .3);
            }
            if (account.Favorability <= 40 && account.Favorability >= 21)
            {
                newFavorability = account.Favorability + (percentageOfBeer * .4);
            }
            if (account.Favorability <= 60 && account.Favorability >= 41)
            {
                newFavorability = account.Favorability + (percentageOfBeer * .5);
            }
            if (account.Favorability <= 80 && account.Favorability >= 61)
            {
                newFavorability = account.Favorability + (percentageOfBeer * .6);
            }

            if (newFavorability > 100)
            {
                newFavorability = 100;
            }

            await this.accountService.SetFavorabilityAsync(account, int.Parse(Math.Floor(newFavorability).ToString()));
            await this.bankService.TransferToFitz(account.Id, int.Parse(amount.ToString()), reason: Features.Bank.Models.Reason.Donated);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("Thanks for the beer.").AsEphemeral(true));
            return;
        }
    }
}