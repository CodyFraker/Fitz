using Fitz.Database;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Database.Entities;
using Fitz.Features.Accounts;
using Fitz.Database.Entities;
using Fitz.Features.Accounts.Queries;
using Fitz.Features.Settings;
using Fitz.Variables;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Favorability.Commands
{
    public class ApplyFavorabilityDropCommand(IServiceScopeFactory scopeFactory, AccountService accountService, SettingsService settingsService)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly AccountService accountService = accountService;
        private readonly SettingsService settingsService = settingsService;

        public async Task<Result> ExecuteAsync(ulong userId)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                var findAccountQuery = new FindAccountQuery(scopeFactory);
                var account = findAccountQuery.Execute(userId);
                if (account == null)
                {
                    return new Result(false, $"Account not found for user {userId}.", null);
                }

                var calculateDropCommand = new CalculateFavorabilityDropCommand(scopeFactory, accountService, settingsService);
                decimal dropPercent = calculateDropCommand.Execute(userId);

                decimal newFavorability = account.Favorability - (account.Favorability * dropPercent / 100);
                int clampedFavorability = Math.Max(0, (int)Math.Floor(newFavorability));

                account.Favorability = clampedFavorability;
                db.Update(account);
                await db.SaveChangesAsync();

                Log.Debug($"Applied favorability drop to {account.Username} ({account.Id}). New favorability: {clampedFavorability}");
                
                return new Result(true, $"Applied favorability drop. New favorability: {clampedFavorability}.", account);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to apply favorability drop for user {userId}.");
                return new Result(false, ex.Message, null);
            }
        }
    }
}
