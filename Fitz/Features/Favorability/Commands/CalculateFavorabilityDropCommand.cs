using Fitz.Database;
using Fitz.Database.Entities;
using Fitz.Features.Accounts;
using Fitz.Database.Entities;
using Fitz.Features.Accounts.Queries;
using Fitz.Features.Settings;
using Fitz.Variables;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Fitz.Features.Favorability.Commands
{
    public class CalculateFavorabilityDropCommand(IServiceScopeFactory scopeFactory, AccountService accountService, SettingsService settingsService)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly AccountService accountService = accountService;
        private readonly SettingsService settingsService = settingsService;

        public decimal Execute(ulong userId)
        {
            var findAccountQuery = new FindAccountQuery(scopeFactory);
            var userAccount = findAccountQuery.Execute(userId);
            var botAccount = findAccountQuery.Execute(Users.Fitz);

            if (userAccount == null || botAccount == null)
            {
                return 0;
            }

            var settings = settingsService.GetSettings();
            int botBeer = Math.Max(botAccount.Beer, 1);
            decimal ratio = (decimal)userAccount.Beer / botBeer;

            if (ratio >= (decimal)settings.FavorabilityBeerRatioThreshold)
            {
                decimal dropPercent = (decimal)settings.FavorabilityBaseDropPercent * 
                    (ratio / (decimal)settings.FavorabilityBeerRatioThreshold) * 
                    (decimal)settings.FavorabilityDropMultiplier;
                return dropPercent;
            }

            return (decimal)settings.FavorabilityBaseDropPercent;
        }
    }
}
