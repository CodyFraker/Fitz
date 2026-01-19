using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Database.Entities;
using Fitz.Database.Entities;
using Fitz.Features.Accounts.Queries;
using Fitz.Features.Bank;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts.Commands
{
    public class GiveBeerCommand(IServiceScopeFactory scopeFactory, BotLog botLog, BankService bankService)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly BotLog botLog = botLog;
        private readonly BankService bankService = bankService;

        public async Task<Result> ExecuteAsync(InteractionContext ctx, double amount)
        {
            var findAccountQuery = new FindAccountQuery(scopeFactory);
            AccountEntity account = findAccountQuery.Execute(ctx.User.Id);

            if (account == null)
            {
                return new Result(false, "You don't have an account yet!", null);
            }

            if (account.Beer < amount)
            {
                return new Result(false, "You don't have enough money to give that much beer!", null);
            }

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

            var setFavorabilityCommand = new SetFavorabilityCommand(scopeFactory, botLog);
            await setFavorabilityCommand.ExecuteAsync(account, int.Parse(Math.Floor(newFavorability).ToString()));
            await this.bankService.TransferToFitz(account.Id, int.Parse(amount.ToString()), reason: Reason.Donated);

            return new Result(true, "Thanks for the beer.", null);
        }
    }
}
