using Fitz.Core.Contexts;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Transaction = Fitz.Features.Bank.Models.Transaction;

namespace Fitz.Features.Bank
{
    public class BankService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private AccountService accountService;

        public BankService(IServiceScopeFactory scopeFactory, AccountService accountService)
        {
            this.scopeFactory = scopeFactory;
            this.accountService = accountService;
        }

        /// <summary>
        /// Award the user 12 beer for creating an account.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task AwardAccountCreationBonus(ulong userId)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Account account = accountService.FindAccount(userId);
            if (account == null)
            {
                Log.Error($"Account not found. {userId}");
                return;
            }

            account.Beer = account.Beer + 128;
            account.LifetimeBeer = account.LifetimeBeer + 128;
            db.Update(account);
            await db.SaveChangesAsync();
            await LogTransactionAsync(account, account, 128, Reason.AccountCreationBonus);
        }

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

            account.Beer = account.Beer + amount;
            account.LifetimeBeer = account.LifetimeBeer + amount;
            db.Update(account);
            await db.SaveChangesAsync();
            await LogTransactionAsync(account, account, amount, Reason.Bonus);
        }

        public List<Transaction> GetTransactions(int take)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            List<Transaction> transactions = db.Transactions.OrderByDescending(t => t.Timestamp).Take(take).ToList();
            return transactions;
        }

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

            account.Beer = account.Beer - amount;
            db.Update(account);
            await LogTransactionAsync(account, account, amount, Reason.Lotto);
            await db.SaveChangesAsync();
        }

        public async Task PurchaseLotteryTicket(Account user, int amount)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            user.Beer -= user.Beer - amount;
            db.Update(user);
            await LogTransactionAsync(user, user, amount, Reason.Lotto);
            await db.SaveChangesAsync();
        }

        public async Task PurchaseRenameAsync(Account user, int amount)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            user.Beer = user.Beer - amount;
            db.Update(user);
            await LogTransactionAsync(user, user, amount, Reason.Rename);
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

            senderAccount.Beer = senderAccount.Beer - amount;
            recipientAccount.LifetimeBeer = recipientAccount.LifetimeBeer + amount;
            recipientAccount.Beer = recipientAccount.Beer + amount;

            db.Update(senderAccount);
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

        public int GetLifetimeBalance(ulong userId)
        {
            Account account = accountService.FindAccount(userId);
            if (account == null)
            {
                Log.Error($"Account not found. {userId}");
                return 0;
            }

            return account.LifetimeBeer;
        }

        public List<Transaction> GetTransactions(ulong userId)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            List<Transaction> transactions = db.Transactions.Where(t => t.Sender == userId || t.Recipient == userId).ToList();
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
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to log transaction.");
            }
        }
    }
}