using Fitz.Core.Contexts;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
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

            account.Beer = account.Beer + 12;
            account.LifetimeBeer = account.LifetimeBeer + 12;
            db.Update(account);
            await db.SaveChangesAsync();
            await LogTransaction(account, account, 12, Reason.AccountCreationBonus);
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
            await LogTransaction(account, account, amount, Reason.Bonus);
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
            await LogTransaction(account, account, amount, Reason.Lotto);
            await db.SaveChangesAsync();
        }

        public async Task PurchaseLotteryTicket(Account user, int amount)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            user.Beer = user.Beer - amount;
            db.Update(user);
            await LogTransaction(user, user, amount, Reason.Lotto);
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

            senderAccount.Beer = senderAccount.Beer - amount;
            recipientAccount.LifetimeBeer = recipientAccount.LifetimeBeer + amount;
            recipientAccount.Beer = recipientAccount.Beer + amount;

            db.Update(senderAccount);
            db.Update(recipientAccount);
            await db.SaveChangesAsync();

            await LogTransaction(senderAccount, recipientAccount, amount, Reason.Donated);
        }

        public async Task<int> GetBalance(ulong userId)
        {
            Account account = accountService.FindAccount(userId);
            if (account == null)
            {
                Log.Error($"Account not found. {userId}");
                return 0;
            }

            return account.Beer;
        }

        private async Task LogTransaction(Account sender, Account recipient, int amount, Reason reason)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
                Transaction transaction = new Transaction()
                {
                    Sender = sender.DiscordId,
                    Recipient = recipient.DiscordId,
                    Amount = amount,
                    Reason = reason,
                    Timestamp = DateTime.Now
                };
                db.Transactions.Add(transaction);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to log transaction.");
            }
        }
    }
}