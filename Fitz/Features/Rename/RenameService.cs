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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Fitz.Features.Rename
{
    public class RenameService(IServiceScopeFactory scopeFactory, AccountService accountService, BankService bankService, SettingsService settingsService, BotLog botLog)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly AccountService accountService = accountService;
        private readonly BankService bankService = bankService;
        private readonly SettingsService settingsService = settingsService;
        private readonly BotLog botLog = botLog;

        public async Task<Result> RenameUserAsync(Account affectedUser, Account requestedUser, string newName, int days, int cost)
        {
            if (affectedUser == null || requestedUser == null)
            {
                return new Result(false, "One of the users does not have an account.", null);
            }

            // Don't allow users to rename the bot.
            if (affectedUser.Username == "Fitz")
            {
                return new Result(false, "You can't rename the bot.", null);
            }

            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            db.Renames.Add(new Renames()
            {
                OldName = affectedUser.Username,
                NewName = newName,
                AffectedUserId = affectedUser.Id,
                RequestedUserId = requestedUser.Id,
                Days = days,
                Expiration = DateTime.Now.AddDays(days),
                Timestamp = DateTime.Now,
            });
            await db.SaveChangesAsync();
            await this.bankService.PurchaseRenameAsync(requestedUser, cost);
            this.botLog.Information(LogConsoleSettings.RenameLog, AccountEmojis.Edit, $"User {requestedUser.Id} has renamed user {affectedUser.Id} with the name {newName} for {days} day(s). Costed: {cost}");
            return new Result(true, "Successfully renamed user.", null);
        }

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

        #region Get Renames

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

        //public int GetHighestRenameCostByAccountId(ulong accountId)
        //{
        //    using IServiceScope scope = scopeFactory.CreateScope();
        //    using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

        //    return db.Renames.Where(x => x.AffectedUserId == accountId).OrderByDescending(x => x.Cost).FirstOrDefault().Cost;
        //}

        #endregion Get Renames

        public int GenerateRenameCost(Account affectedUser, Account requestedUser, double daysOfRename, string newName)
        {
            if (affectedUser.Username == "Fitz")
            {
                return 999999999;
            }

            Settings settings = this.settingsService.GetSettings();

            double baseCost = (double)settings.RenameBaseCost;

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
                baseCost = baseCost * 1.2;
            }
            return int.Parse(Math.Ceiling(baseCost * daysOfRename).ToString());
        }
    }
}