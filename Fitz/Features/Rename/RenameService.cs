using Fitz.Core.Contexts;
using Fitz.Core.Models;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Features.Rename.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Rename
{
    public class RenameService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly AccountService accountService;
        private readonly BankService bankService;

        public RenameService(IServiceScopeFactory scopeFactory, AccountService accountService, BankService bankService)
        {
            this.scopeFactory = scopeFactory;
            this.accountService = accountService;
            this.bankService = bankService;
        }

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
            return new Result(true, "Successfully renamed user.", null);
        }

        #region Get Renames

        public async void GetExpiredRenames()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            List<Renames> startDateRenames = db.Renames.Where(x => x.Expiration == null).ToList();

            foreach (Renames rename in startDateRenames)
            {
                if (rename.Timestamp.AddDays((double)rename.Days) < DateTime.Now)
                {
                    rename.Expiration = rename.Timestamp.AddDays((double)rename.Days);
                    db.Renames.Update(rename);
                    await db.SaveChangesAsync();
                }
            }
        }

        public List<Renames> GetRenameRequestsByAccount(Account account)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return db.Renames.Where(x => x.RequestedUserId == account.Id).ToList();
        }

        #endregion Get Renames

        public int GenerateRenameCost(Account affectedUser, Account requestedUser, double daysOfRename, string newName)
        {
            if (affectedUser.Username == "Fitz")
            {
                return 999999999;
            }
            double baseCost = 6;

            // The less the bot likes the affected user, the cheaper the cost is.
            if (affectedUser.Favorability <= 5)
            {
                baseCost = baseCost * 1;
            }
            if (affectedUser.Favorability <= 20 && affectedUser.Favorability >= 6)
            {
                baseCost = baseCost * 0.8;
            }
            if (affectedUser.Favorability <= 40 && affectedUser.Favorability >= 21)
            {
                baseCost = baseCost * 0.6;
            }
            if (affectedUser.Favorability <= 60 && affectedUser.Favorability >= 41)
            {
                baseCost = baseCost * 0.4;
            }
            if (affectedUser.Favorability <= 80 && affectedUser.Favorability >= 61)
            {
                baseCost = baseCost * 0.2;
            }
            // If the bot really likes the affected user, the cost is more expensive.
            if (affectedUser.Favorability <= 95 && affectedUser.Favorability >= 81)
            {
            }

            // The more the bot doesn't like the requested user, the more expensive the rename.
            if (requestedUser.Favorability == 0)
            {
                baseCost = baseCost * 100;
            }
            if (requestedUser.Favorability <= 20 && requestedUser.Favorability >= 6)
            {
                baseCost = baseCost / .1;
            }
            if (requestedUser.Favorability <= 40 && requestedUser.Favorability >= 21)
            {
                baseCost = baseCost * .2;
            }
            if (requestedUser.Favorability <= 60 && requestedUser.Favorability >= 41)
            {
                baseCost = baseCost * .4;
            }
            if (requestedUser.Favorability <= 80 && requestedUser.Favorability >= 61)
            {
                baseCost = baseCost * .8;
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