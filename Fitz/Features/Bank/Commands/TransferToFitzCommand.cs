using Fitz.Database;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Database.Entities;
using Fitz.Features.Accounts;
using Fitz.Database.Entities;
using Fitz.Features.Accounts.Queries;
using Fitz.Database.Entities;
using Fitz.Features.Favorability;
using Fitz.Features.Settings;
using Fitz.Metrics;
using Fitz.Variables;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Transaction = Fitz.Database.Entities.Transaction;

namespace Fitz.Features.Bank.Commands
{
    public class TransferToFitzCommand(IServiceScopeFactory scopeFactory, AccountService accountService, SettingsService settingsService, FavorabilityService favorabilityService, BotLog botLog, FitzMetrics? fitzMetrics = null)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly AccountService accountService = accountService;
        private readonly SettingsService settingsService = settingsService;
        private readonly FavorabilityService favorabilityService = favorabilityService;
        private readonly BotLog botLog = botLog;
        private readonly FitzMetrics? fitzMetrics = fitzMetrics;

        public async Task<Result> ExecuteAsync(ulong userId, int amount, Reason reason)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                var findAccountQuery = new Accounts.Queries.FindAccountQuery(scopeFactory);
                var account = findAccountQuery.Execute(userId);
                if (account == null)
                {
                    return new Result(false, $"{userId} did not have an account.", null);
                }

                if (!favorabilityService.CanUserExecuteCommands(userId))
                {
                    return new Result(false, "I refuse to perform this action. Your favorability with me is at zero. Give me some beer using `/beer` to improve our relationship.", null);
                }

                decimal costMultiplier = favorabilityService.CalculateCostMultiplier(userId);
                int finalAmount = (int)Math.Ceiling(amount * costMultiplier);

                if (account.Beer < finalAmount)
                {
                    if (costMultiplier > 1.0m)
                    {
                        decimal ratio = favorabilityService.CalculateBeerRatio(userId);
                        return new Result(false, $"You have {ratio:F2}x more beer than me. This will cost you {amount} × {costMultiplier:F2} = {finalAmount} beer. You don't have enough beer.", null);
                    }
                    return new Result(false, $"{userId} did not have enough beer to transfer.", null);
                }

                var fitz = findAccountQuery.Execute(Users.Fitz);

                account.Beer -= finalAmount;
                fitz.Beer += finalAmount;
                fitz.LifetimeBeer += finalAmount;

                db.Update(account);
                await db.SaveChangesAsync();
                db.Update(fitz);
                await db.SaveChangesAsync();
                
                var logTransactionCommand = new LogTransactionCommand(scopeFactory, botLog);
                await logTransactionCommand.ExecuteAsync(account, fitz, finalAmount, reason);
                
                fitzMetrics?.RecordBeerDeduction(finalAmount, reason.ToString());
                fitzMetrics?.RecordTransaction("transfer_to_fitz");
                
                await favorabilityService.ApplyFavorabilityDropAsync(userId);
                
                string message = costMultiplier > 1.0m 
                    ? $"Transferred {finalAmount} beer to Fitz (original cost: {amount}, multiplier: {costMultiplier:F2}x)." 
                    : $"Transferred {finalAmount} beer to Fitz.";
                
                return new Result(true, message, account);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }
    }
}
