using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Core.Services.Settings;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank.Models;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transaction = Fitz.Features.Bank.Models.Transaction;

namespace Fitz.Features.Bank
{
    public class BankService(IServiceScopeFactory scopeFactory, AccountService accountService, SettingsService settingsService, BotLog botLog)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly AccountService accountService = accountService;
        private readonly SettingsService settingsService = settingsService;
        private readonly BotLog botLog = botLog;

        #region Account Creation Bonus

        /// <summary>
        /// Award the user beer for creating an account.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<Result> AwardAccountCreationBonusAsync(Account account)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
                Settings settings = this.settingsService.GetSettings();

                account.Beer += settings.AccountCreationBonusAmount;
                account.LifetimeBeer += settings.AccountCreationBonusAmount;

                db.Update(account);
                await db.SaveChangesAsync();
                await LogTransactionAsync(account, account, settings.AccountCreationBonusAmount, Reason.AccountCreationBonus);
                return new Result(true, $"Awarded account creation bonus to {account.Username}.", account);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }

        #endregion Account Creation Bonus

        #region Award Bonus Generic

        /// <summary>
        /// Used for things such as winning the lottery
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task AwardBonus(ulong userId, int amount)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Account account = accountService.FindAccount(userId);
            if (account == null)
            {
                Log.Error($"Account not found. {userId}");
                return;
            }

            account.Beer += amount;
            account.LifetimeBeer += amount;
            db.Update(account);
            await db.SaveChangesAsync();
            await LogTransactionAsync(account, account, amount, Reason.Bonus);
        }

        #endregion Award Bonus Generic

        #region Award Happy Hour

        public async Task<Result> AwardHappyHour(ulong userId)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
                Settings settings = this.settingsService.GetSettings();

                Account account = accountService.FindAccount(userId);
                if (account == null)
                {
                    return new Result(false, $"{userId} did not have an account.", null);
                }

                account.Beer += settings.BaseHappyHourAmount;
                account.LifetimeBeer += settings.BaseHappyHourAmount;
                db.Update(account);
                await db.SaveChangesAsync();
                await LogTransactionAsync(account, account, settings.BaseHappyHourAmount, Reason.HappyHour);

                return new Result(true, $"Awarded happy hour bonus to {account.Username}.", account);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }

        #endregion Award Happy Hour

        #region Polls

        #region Award Poll Vote

        public async Task<Result> AwardPollVote(ulong userId)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
                Settings settings = this.settingsService.GetSettings();

                Account account = accountService.FindAccount(userId);
                if (account == null)
                {
                    return new Result(false, $"{userId} did not have an account.", null);
                }

                account.Beer += settings.PollVote;
                account.LifetimeBeer += settings.PollVote;
                db.Update(account);
                await db.SaveChangesAsync();
                await LogTransactionAsync(account, account, settings.PollVote, Reason.PollVote);

                return new Result(true, $"Awarded poll vote bonus to {account.Username}.", account);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }

        #endregion Award Poll Vote

        #region Tip Poll Creator Vote

        public async Task<Result> TipPollCreatorVote(ulong accountId)
        {
            try
            {
                if (accountId == Users.Fitz)
                {
                    return new Result(false, "Cannot tip Fitz.", null);
                }
                if (accountId == Users.Dodecuplet)
                {
                    return new Result(false, "Cannot tip Dodecuplet.", null);
                }

                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
                Settings settings = this.settingsService.GetSettings();

                Account account = accountService.FindAccount(accountId);
                if (account == null)
                {
                    return new Result(false, $"{accountId} did not have an account.", null);
                }

                account.Beer += settings.PollCreatorTip;
                account.LifetimeBeer += settings.PollCreatorTip;
                db.Update(account);
                await db.SaveChangesAsync();
                await LogTransactionAsync(account, account, settings.PollCreatorTip, Reason.PollCreatorTip);

                return new Result(true, $"Tipped poll creator vote bonus to {account.Username}.", account);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }

        #endregion Tip Poll Creator Vote

        #region Award Poll Approved

        /// <summary>
        /// Award a user when a submitted poll is approved.
        /// </summary>
        /// <param name="userId">Their account ID</param>
        /// <returns></returns>
        public async Task<Result> AwardPollApproval(ulong userId)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
                Settings settings = this.settingsService.GetSettings();

                Account account = accountService.FindAccount(userId);
                if (account == null)
                {
                    return new Result(false, $"{userId} did not have an account.", null);
                }

                account.Beer += settings.PollApprovedBonus;
                account.LifetimeBeer += settings.PollApprovedBonus;
                db.Update(account);
                await db.SaveChangesAsync();
                await LogTransactionAsync(account, account, settings.PollApprovedBonus, Reason.PollApproved);

                return new Result(true, $"Awarded poll approval bonus to {account.Username}.", account);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }

        #endregion Award Poll Approved

        #region Poll Declined Penalty

        public async Task<Result> DeclineUserPoll(ulong userId)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
                Settings settings = this.settingsService.GetSettings();

                Account account = accountService.FindAccount(userId);
                if (account == null)
                {
                    return new Result(false, $"{userId} did not have an account.", null);
                }

                account.Beer -= settings.PollDeclinedPenalty;
                db.Update(account);
                await db.SaveChangesAsync();
                await LogTransactionAsync(account, account, settings.PollDeclinedPenalty, Reason.PollDeclined);

                return new Result(true, $"Deducted poll declined bonus to {account.Username}.", account);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }

        #endregion Poll Declined Penalty

        #region Poll Submitted Penalty

        public async Task<Result> UserSubmittedPollPenalty(ulong userId)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
                Settings settings = this.settingsService.GetSettings();

                Account account = accountService.FindAccount(userId);
                if (account == null)
                {
                    return new Result(false, $"{userId} did not have an account.", null);
                }

                account.Beer -= settings.PollSubmittedPenalty;
                db.Update(account);
                await db.SaveChangesAsync();
                await LogTransactionAsync(account, account, settings.PollSubmittedPenalty, Reason.PollSubmitted);

                return new Result(true, $"Deducted poll submitted penalty to {account.Username}.", account);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }

        #endregion Poll Submitted Penalty

        #endregion Polls

        #region Transactions

        public List<Transaction> GetTransactions(int take)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            List<Transaction> transactions = db.Transactions.OrderByDescending(t => t.Timestamp).Take(take).ToList();
            return transactions;
        }

        #endregion Transactions

        #region Lottery

        public async Task PurchaseLotteryTicket(ulong userId, int amount)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Account account = accountService.FindAccount(userId);
            if (account == null)
            {
                Log.Error($"Account not found. {userId}");
                return;
            }

            account.Beer -= amount;
            db.Update(account);
            await LogTransactionAsync(account, account, amount, Reason.Lotto);
            await db.SaveChangesAsync();
        }

        public async Task PurchaseLotteryTicket(Account user, int amount)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            user.Beer -= amount;
            db.Update(user);
            await LogTransactionAsync(user, user, amount, Reason.Lotto);
            await db.SaveChangesAsync();
        }

        public async Task DepositLotteryWinningsAsync(Account account, int amount)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            account.Beer += amount;
            account.LifetimeBeer += amount;

            db.Update(account);
            await db.SaveChangesAsync();
            await LogTransactionAsync(account, account, amount, Reason.LottoWin);
        }

        #endregion Lottery

        public async Task<Result> TransferToFitz(ulong userId, int amount)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Account account = accountService.FindAccount(userId);
            if (account == null)
            {
                return new Result(false, $"{userId} did not have an account.", null);
            }

            if (account.Beer < amount)
            {
                return new Result(false, $"{userId} did not have enough beer to transfer.", null);
            }
            Account Fitz = accountService.FindAccount(Users.Fitz);

            account.Beer -= amount;
            Fitz.Beer += amount;
            Fitz.LifetimeBeer += amount;

            db.Update(account);
            await db.SaveChangesAsync();
            db.Update(Fitz);
            await db.SaveChangesAsync();
            await LogTransactionAsync(account, Fitz, amount, Reason.Donated);

            return new Result(true, $"Transferred {amount} beer to Fitz.", account);
        }

        public async Task PurchaseRenameAsync(Account user, int amount)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            user.Beer -= amount;
            db.Update(user);
            await LogTransactionAsync(user, user, amount, Reason.Rename);
            await db.SaveChangesAsync();
        }

        public async Task TransferBeer(ulong sender, ulong recipient, int amount)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Account senderAccount = accountService.FindAccount(sender);
            if (senderAccount == null)
            {
                Log.Error($"Sender account not found. {sender}");
                return;
            }

            Account recipientAccount = accountService.FindAccount(recipient);
            if (recipientAccount == null)
            {
                Log.Error($"Recipient account not found. {recipient}");
                return;
            }

            // Check to see if sender has enough beer to give.
            if (senderAccount.Beer < amount)
            {
                Log.Error($"Sender does not have enough beer to give. {sender}");
                return;
            }

            senderAccount.Beer -= amount;
            db.Update(senderAccount);
            await db.SaveChangesAsync();
            recipientAccount.LifetimeBeer += amount;
            recipientAccount.Beer += amount;
            db.Update(recipientAccount);
            await db.SaveChangesAsync();

            await LogTransactionAsync(senderAccount, recipientAccount, amount, Reason.Donated);
        }

        public int GetBalance(ulong userId)
        {
            Account account = accountService.FindAccount(userId);
            if (account == null)
            {
                Log.Error($"Account not found. {userId}");
                return 0;
            }

            return account.Beer;
        }

        public List<Transaction> GetTransactions(ulong userId)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            List<Transaction> transactions = db.Transactions.Where(t => t.Sender == userId || t.Recipient == userId).OrderByDescending(x => x.Timestamp).ToList();
            return transactions;
        }

        public List<Account> GetTopBeerBalances(int limit = 10)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            List<Account> accounts = db.Accounts.OrderByDescending(a => a.Beer).Take(limit).ToList();
            return accounts;
        }

        private async Task LogTransactionAsync(Account sender, Account recipient, int amount, Reason reason)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                Transaction transaction = new Transaction()
                {
                    Sender = sender.Id,
                    Recipient = recipient.Id,
                    Amount = amount,
                    Reason = reason,
                    Timestamp = DateTime.Now
                };
                db.Add(transaction);
                await db.SaveChangesAsync();
                Log.Debug($"Transaction logged: Sender: {sender} | Recipient: {recipient} | Amount: {amount}, Reason: {reason}");
                this.botLog.Information(LogConsoleSettings.Transactions, BankEmojis.Transaction, $"Transaction logged: Sender: {sender} | Recipient: {recipient} | Amount: {amount}, Reason: {reason}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to log transaction.");
            }
        }
    }
}