using Fitz.Database;
using Fitz.Database.Entities;
using Fitz.Features.Accounts.Queries;
using Fitz.Features.Settings;
using Fitz.Variables;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Fitz.Features.Favorability.Queries
{
    public class GetFavorabilityStatusQuery(IServiceScopeFactory scopeFactory, SettingsService settingsService)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly SettingsService settingsService = settingsService;

        public FavorabilityStatus Execute(ulong userId)
        {
            var findAccountQuery = new Accounts.Queries.FindAccountQuery(scopeFactory);
            var userAccount = findAccountQuery.Execute(userId);
            var botAccount = findAccountQuery.Execute(Users.Fitz);

            if (userAccount == null || botAccount == null)
            {
                return new FavorabilityStatus
                {
                    UserId = userId,
                    CanUseCommands = false,
                    BeerRatio = 0,
                    Favorability = 0,
                    CostMultiplier = 1.0m
                };
            }

            var settings = settingsService.GetSettings();
            int botBeer = Math.Max(botAccount.Beer, 1);
            decimal beerRatio = (decimal)userAccount.Beer / botBeer;
            bool canUseCommands = userAccount.Favorability > 0;

            decimal costMultiplier = 1.0m;
            if (beerRatio >= (decimal)settings.FavorabilityBeerRatioThreshold)
            {
                costMultiplier = 1 + ((beerRatio - (decimal)settings.FavorabilityBeerRatioThreshold) * 0.1m);
                costMultiplier = Math.Max(1.0m, Math.Min(3.0m, costMultiplier));
            }

            return new FavorabilityStatus
            {
                UserId = userId,
                CanUseCommands = canUseCommands,
                BeerRatio = beerRatio,
                Favorability = userAccount.Favorability,
                CostMultiplier = costMultiplier
            };
        }
    }

    public class FavorabilityStatus
    {
        public ulong UserId { get; set; }
        public bool CanUseCommands { get; set; }
        public decimal BeerRatio { get; set; }
        public int Favorability { get; set; }
        public decimal CostMultiplier { get; set; }
    }
}
