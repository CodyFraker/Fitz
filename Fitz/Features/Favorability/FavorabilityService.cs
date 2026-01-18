using Fitz.Core.Models;
using Fitz.Database;
using Fitz.Database.Entities;
using Fitz.Features.Accounts;
using Fitz.Features.Favorability.Commands;
using Fitz.Features.Settings;
using Fitz.Variables;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Favorability
{
    public class FavorabilityService(IServiceScopeFactory scopeFactory, AccountService accountService, SettingsService settingsService)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly AccountService accountService = accountService;
        private readonly SettingsService settingsService = settingsService;

        public decimal CalculateBeerRatio(ulong userId)
        {
            var findAccountQuery = new Accounts.Queries.FindAccountQuery(scopeFactory);
            var userAccount = findAccountQuery.Execute(userId);
            var botAccount = findAccountQuery.Execute(Users.Fitz);

            if (userAccount == null || botAccount == null)
            {
                return 0;
            }

            int botBeer = Math.Max(botAccount.Beer, 1);
            return (decimal)userAccount.Beer / botBeer;
        }

        public decimal CalculateCostMultiplier(ulong userId)
        {
            var settings = settingsService.GetSettings();
            decimal ratio = CalculateBeerRatio(userId);

            if (ratio >= (decimal)settings.FavorabilityBeerRatioThreshold)
            {
                decimal multiplier = 1 + ((ratio - (decimal)settings.FavorabilityBeerRatioThreshold) * 0.1m);
                return Math.Max(1.0m, Math.Min(3.0m, multiplier));
            }

            return 1.0m;
        }

        public bool CanUserExecuteCommands(ulong userId)
        {
            var findAccountQuery = new Accounts.Queries.FindAccountQuery(scopeFactory);
            var account = findAccountQuery.Execute(userId);
            return account != null && account.Favorability > 0;
        }

        public async Task<Result> ApplyFavorabilityDropAsync(ulong userId)
        {
            var command = new ApplyFavorabilityDropCommand(scopeFactory, accountService, settingsService);
            return await command.ExecuteAsync(userId);
        }

        public decimal CalculateFavorabilityDropPercent(ulong userId)
        {
            var command = new CalculateFavorabilityDropCommand(scopeFactory, accountService, settingsService);
            return command.Execute(userId);
        }
    }
}
