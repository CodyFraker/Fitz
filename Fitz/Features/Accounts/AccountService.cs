using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Features.Accounts.Models;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts
{
    public sealed class AccountService(IServiceScopeFactory scopeFactory, BotLog botLog)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly BotLog botLog = botLog;

        #region Account Creation

        public async Task<Result> CreateAccountAsync(DiscordUser user)
        {
            // Check to see if the user already has an account.
            if (FindAccount(user.Id) != null)
            {
                return new Result(true, "You already have an account.", FindAccount(user.Id));
            }

            // Setup default account details.
            Account account = new()
            {
                Id = user.Id,
                Username = user.Username,
                Beer = 0,
                LifetimeBeer = 0,
                safeBalance = 128,
                Favorability = 50,
                CreatedDate = DateTime.Now,
                LastSeenDate = DateTime.Now,
                LastActivityDate = DateTime.Now,
                subscribeToLottery = false,
                SubscribeTickets = 1,
            };

            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                // Add account to the database
                db.Accounts.Add(account);

                // Save changes
                await db.SaveChangesAsync();

                Log.Debug($"Added new account to Database: {user.Username} | {user.Id}");
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Add, $"Created a new account for: {user.Username} | {user.Id}");
                return new Result(true, "Account created successfully.", account);
            }
            catch (Exception e)
            {
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Warning, $"Error creating a new account for: {user.Username} | {user.Id}");
                Log.Error(e, $"Couldn't add new account! {user.Username} | {user.Id}");
                return new Result(false, "Failed to create account.", account);
            }
        }

        public async Task<Result> CreateFitzAccountAsync()
        {
            Account account = new Account()
            {
                Id = Users.Fitz,
                Username = "Fitz",
                LifetimeBeer = 128,
                Beer = 128,
                Favorability = 100,
                CreatedDate = DateTime.Now,
                LastSeenDate = DateTime.Now,
                LastActivityDate = DateTime.Now,
                subscribeToLottery = false,
                SubscribeTickets = 1,
            };

            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
                db.Accounts.Add(account);
                await db.SaveChangesAsync();
                Log.Debug($"Created an account for Fitz");
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Add, $"Fitz account not found in database. Created an account for Fitz.");
                return new Result(true, "Account created successfully.", account);
            }
            catch (Exception e)
            {
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Warning, $"Failed to create an account for Fitz. {e.StackTrace}");
                Log.Error(e, $"Failed to create an account for Fitz.");
                return new Result(false, "Failed to create account.", account);
            }
        }

        #endregion Account Creation

        #region Account Updates

        #region Set Safe Balance

        public async Task<Result> SetSafeBalanceAsync(Account account, int safeBalance)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            if (account == null)
            {
                return new Result(false, "Account settings not found.", account);
            }

            account.safeBalance = safeBalance;

            try
            {
                db.Accounts.Update(account);
                await db.SaveChangesAsync();
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Edit, $"Updated safe balance for {account.Username} | {account.Id} to {safeBalance}");
                return new Result(true, "Safe balance updated successfully.", account);
            }
            catch (Exception e)
            {
                Log.Error(e, $"Failed to update safe balance for {account.Id}.");
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Warning, $"Failed to update safe balance for {account.Username} | {account.Id} | Stack trace: {e.StackTrace}");
                return new Result(false, "Failed to update safe balance.", account);
            }
        }

        public async Task<Result> SetSafeBalanceAsync(ulong userId, int safeBalance)
        {
            return await SetSafeBalanceAsync(FindAccount(userId), safeBalance);
        }

        #endregion Set Safe Balance

        #region Set Lottery Subscription

        public async Task<Result> SetLotterySubscribe(Account account, bool subscribe)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
            account.subscribeToLottery = subscribe;

            try
            {
                db.Accounts.Update(account);
                await db.SaveChangesAsync();
                Log.Debug($"Updated lottery subscription for {account.Username} | {account.Id} to {subscribe}");
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Edit, $"Updated lottery subscription for {account.Username} | {account.Id} to {subscribe}");
                return new Result(true, "Lottery subscription updated successfully.", account);
            }
            catch (Exception e)
            {
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Warning, $"Failed to update lottery subscription for {account.Username} | {account.Id} | Stack trace: {e.StackTrace}");
                Log.Error(e, $"Failed to update lottery subscription for {account.Id}.");
                return new Result(false, "Failed to update lottery subscription.", account);
            }
        }

        #endregion Set Lottery Subscription

        #region Set Lottery Ticket Amount

        public async Task<Result> SetTicketAmountAsync(Account Account, int Amount)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
            try
            {
                Account.SubscribeTickets = Amount;
                db.Update(Account);
                await db.SaveChangesAsync();
                Log.Debug($"Updated ticket amount for {Account.Username} | {Account.Id} to {Amount}");
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Edit, $"Updated ticket amount for {Account.Username} | {Account.Id} to {Amount}");
                return new Result(true, "Ticket amount updated successfully.", Account);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to update ticket amount for {Account.Id}.");
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Warning, $"Failed to update ticket amount for {Account.Username} | {Account.Id} | Stack trace: {ex.StackTrace}");
                return new Result(false, ex.Message, null);
            }
        }

        #endregion Set Lottery Ticket Amount

        #region Set Favorability

        public async Task<Result> SetFavorabilityAsync(Account account, int newFavorability)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                if (account.Favorability >= 100)
                {
                    return new Result(false, "User already has max favorability.", account);
                }
                else
                {
                    account.Favorability = newFavorability;
                    db.Accounts.Update(account);
                    await db.SaveChangesAsync();
                    Log.Debug($"{newFavorability} Favorability added to {account.Id} successfully.");
                    this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Edit, $"{newFavorability} Favorability added to {account.Username} | {account.Id} successfully.");
                    return new Result(true, $"Favorability added to {account.Id} successfully.", account);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to update favorability for {account.Id}.");
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Warning, $"Failed to update favorability for {account.Username} | {account.Id} | Stack trace: {ex.StackTrace}");
                return new Result(false, ex.Message, null);
            }
        }

        #endregion Set Favorability

        #region Set Username

        public async Task<Result> SetUsernameAsync(Account account, string username)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                if (account == null)
                {
                    return new Result(false, "Account not found.", account);
                }

                account.Username = username;

                db.Accounts.Update(account);
                await db.SaveChangesAsync();
                Log.Debug($"Updated username for {account.Id} to {username}");
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Edit, $"Updated username for {account.Username} | {account.Id} to {username}");
                return new Result(true, "Username updated successfully.", account);
            }
            catch (Exception e)
            {
                Log.Error(e, $"Failed to update username for {account.Id}.");
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Warning, $"Failed to update username for {account.Username} | {account.Id} | Stack trace: {e.StackTrace}");
                return new Result(false, "Failed to update username.", account);
            }
        }

        #endregion Set Username

        #endregion Account Updates

        #region Query & Find Accounts

        /// <summary>
        /// Returns a list of all accounts.
        /// </summary>
        /// <returns>All Accounts.</returns>
        public List<Account> QueryAccounts()
        {
            List<Account> dbAccounts = [];

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

        public List<Account> GetLotterySubscribers()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
            return [.. db.Accounts.Where(x => x.subscribeToLottery == true)];
        }

        public Account FindAccount(ulong id)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
            return db.Accounts.Where(x => x.Id == id).FirstOrDefault();
        }

        public Account FindAccount(DiscordUser user)
        {
            return FindAccount(user.Id);
        }

        #endregion Query & Find Accounts

        #region Embeds

        public DiscordEmbed AccountHelpEmbed(DiscordClient dClient)
        {
            DiscordEmbedBuilder accountHelpEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(dClient, AccountEmojis.Users).Url,
                    Text = $"Account Command Help",
                },
                Color = new DiscordColor(52, 114, 53),
                Timestamp = DateTime.UtcNow,
                Title = "Account Command Help",
                Description = "**Commands**\n" +
                $"`/signup`: Create an account with me. Everyone needs one.\n" +
                $"`/settings`: Edit your account settings.\n" +
                $"`/account`: View your account details\n"
            };

            return accountHelpEmbed.Build();
        }

        #endregion Embeds
    }
}