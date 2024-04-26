using DSharpPlus.Entities;
using Fitz.Core.Contexts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts
{
    public sealed class AccountService
    {
        private readonly IServiceScopeFactory scopeFactory;

        public AccountService(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }

        public async Task<Account> CreateAccount(Account account)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            db.Accounts.Add(account);
            await db.SaveChangesAsync();

            return account;
        }

        public async Task AddAsync(DiscordUser user, DateTime accountCreated)
        {
            // Before adding the user into the database, we want to give that user some base properties.
            Account account = new Account()
            {
                Id = user.Id,
                Username = user.Username,
                CreatedDate = DateTime.Now,
                LifetimeBeer = 0,
                Beer = 0,
                Favorability = 50,
                Renames = 0,
            };

            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
                db.Accounts.Add(account);
                await db.SaveChangesAsync();

                Log.Debug($"Added new account to Database: {user.Username}#{user.Discriminator} | {user.Id}");
            }
            catch (Exception e)
            {
                // Catch if this doesn't happen which should never happen but thats not very good reason to not include this.
                Log.Error(e, $"Couldn't add new account! {user.Username}#{user.Discriminator} | {user.Id}");
                return;
            }
        }

        /// <summary>
        /// Returns a list of package accounts.
        /// </summary>
        /// <returns>All Package Accounts.</returns>
        public List<Account> QueryAccounts()
        {
            List<Account> dbAccounts = new List<Account>();

            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
            try
            {
                DbSet<Account> dbQuery = db.Accounts;
                foreach (Account account in dbQuery)
                {
                    dbAccounts.Add(account);
                }

                return dbAccounts;
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to query all accounts!");
            }

            return dbAccounts;
        }

        public Account FindAccount(ulong id)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
            return db.Accounts.Where(x => x.Id == id).FirstOrDefault();
        }
    }
}