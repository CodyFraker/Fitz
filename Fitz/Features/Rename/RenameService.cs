using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Core.Services.Settings;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Features.Rename.Models;
using Fitz.Variables.Emojis;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Rename
{
    public class RenameService(IServiceScopeFactory scopeFactory, AccountService accountService, BankService bankService, SettingsService settingsService, BotLog botLog)
    {
        #region Private Members

        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly AccountService accountService = accountService;
        private readonly BankService bankService = bankService;
        private readonly SettingsService settingsService = settingsService;
        private readonly BotLog botLog = botLog;

        #endregion Private Members

        #region Create Rename

        public async Task<Result> RenameUserAsync(Renames rename)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                db.Renames.Add(rename);
                await db.SaveChangesAsync();
                await this.bankService.PurchaseRenameAsync(rename.RequestedUserId, rename.Cost);
                this.botLog.Information(LogConsoleSettings.RenameLog, AccountEmojis.Edit, $"User {rename.RequestedUserId} has renamed user {rename.AffectedUserId} with the name {rename.NewName} for {rename.Days} day(s). Costed: {rename.Cost}");
                return new Result(true, "Successfully renamed user.", null);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }

        #endregion Create Rename

        #region Set Renames

        public async Task<Result> SetUserNotified(Renames rename)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                rename.Notified = true;
                db.Renames.Update(rename);
                await db.SaveChangesAsync();
                this.botLog.Information(LogConsoleSettings.RenameLog, AccountEmojis.Edit, $"User {rename.RequestedUserId} has been notified");
                return new Result(true, "Successfully updated user notification.", rename);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }

        public async Task<Result> BuyoutRenameRequests(ulong accountId)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
                List<Renames> renames = this.GetRenamesByAccountId(accountId);
                foreach (Renames rename in renames)
                {
                    rename.Status = RenameStatus.BoughtOut;
                    db.Renames.Update(rename);
                    await db.SaveChangesAsync();
                }
                this.botLog.Information(LogConsoleSettings.RenameLog, AccountEmojis.Edit, $"User {accountId} has bought out all rename requests.");
                return new Result(true, "Successfully bought out all rename requests.", null);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }

        #endregion Set Renames

        #region Get Renames

        public List<Renames> GetRenamesByAccountId(ulong accountId)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
            return db.Renames.Where(x => x.AffectedUserId == accountId && x.Status != RenameStatus.Expired).ToList();
        }

        public List<Renames> GetExpiredRenames()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
            return db.Renames.Where(x => x.Expiration < DateTime.Now).ToList();
        }

        public int GetTotalRenameRequestsByAccountId(ulong accountId)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return db.Renames.Where(x => x.RequestedUserId == accountId).Count();
        }

        public int GetTotalRenamesByAccountId(ulong accountId)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return db.Renames.Where(x => x.AffectedUserId == accountId).Count();
        }

        #endregion Get Renames

        #region Generate Rename Cost

        public int GenerateRenameCost(Account affectedUser, Account requestedUser, double daysOfRename, string newName)
        {
            if (affectedUser.Username == "Fitz")
            {
                return 999999999;
            }

            Settings settings = this.settingsService.GetSettings();

            double baseCost = (double)settings.RenameBaseCost;

            if (affectedUser.Id == requestedUser.Id)
            {
                baseCost += 150;
            }

            // The less the bot likes the affected user, the cheaper the cost is.
            if (affectedUser.Favorability <= 5)
            {
                baseCost *= 1;
            }
            if (affectedUser.Favorability <= 20 && affectedUser.Favorability >= 6)
            {
                baseCost *= 0.8;
            }
            if (affectedUser.Favorability <= 40 && affectedUser.Favorability >= 21)
            {
                baseCost *= 0.6;
            }
            if (affectedUser.Favorability <= 60 && affectedUser.Favorability >= 41)
            {
                baseCost *= 0.4;
            }
            if (affectedUser.Favorability <= 80 && affectedUser.Favorability >= 61)
            {
                baseCost *= 0.2;
            }
            // If the bot really likes the affected user, the cost is more expensive.
            if (affectedUser.Favorability <= 95 && affectedUser.Favorability >= 81)
            {
            }

            // The more the bot doesn't like the requested user, the more expensive the rename.
            if (requestedUser.Favorability == 0)
            {
                baseCost *= 100;
            }
            if (requestedUser.Favorability <= 20 && requestedUser.Favorability >= 6)
            {
                baseCost /= .1;
            }
            if (requestedUser.Favorability <= 40 && requestedUser.Favorability >= 21)
            {
                baseCost *= .2;
            }
            if (requestedUser.Favorability <= 60 && requestedUser.Favorability >= 41)
            {
                baseCost *= .4;
            }
            if (requestedUser.Favorability <= 80 && requestedUser.Favorability >= 61)
            {
                baseCost *= .8;
            }
            if (requestedUser.Favorability <= 95 && requestedUser.Favorability >= 81)
            {
            }

            foreach (char c in newName)
            {
                baseCost *= 1.2;
            }
            return int.Parse(Math.Ceiling(baseCost * daysOfRename).ToString());
        }

        #endregion Generate Rename Cost
    }
}