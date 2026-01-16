using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank.Commands;
using Fitz.Features.Bank.Models;
using Fitz.Features.Bank.Queries;
using Fitz.Features.Settings;
using Fitz.Variables;
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

        public async Task<Result> AwardAccountCreationBonusAsync(Account account)
        {
            var command = new AwardAccountCreationBonusCommand(scopeFactory, botLog);
            return await command.ExecuteAsync(account);
        }

        #region Award Bonus Generic

        /// <summary>
        /// Used for things such as winning the lottery
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<Result> AwardBonus(ulong userId, int amount)
        {
            var command = new AwardBonusCommand(scopeFactory, accountService, botLog);
            return await command.ExecuteAsync(userId, amount);
        }

        #endregion Award Bonus Generic

        #region Deduct Beer

        public async Task<Result> DeductBeerFromUser(ulong userId, int amount, Reason reason)
        {
            var command = new DeductBeerCommand(scopeFactory, accountService, botLog);
            return await command.ExecuteAsync(userId, amount, reason);
        }

        #endregion Deduct Beer

        #region Award Happy Hour

        public async Task<Result> AwardHappyHour(ulong userId)
        {
            var command = new AwardHappyHourCommand(scopeFactory, accountService, settingsService, botLog);
            return await command.ExecuteAsync(userId);
        }

        #endregion Award Happy Hour

        #region Polls

        #region Award Poll Vote

        public async Task<Result> AwardPollVote(ulong userId)
        {
            var command = new AwardPollVoteCommand(scopeFactory, accountService, settingsService, botLog);
            return await command.ExecuteAsync(userId);
        }

        #endregion Award Poll Vote

        #region Tip Poll Creator Vote

        public async Task<Result> TipPollCreatorVote(ulong accountId)
        {
            var command = new TipPollCreatorVoteCommand(scopeFactory, accountService, settingsService, botLog);
            return await command.ExecuteAsync(accountId);
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
            var command = new AwardPollApprovalCommand(scopeFactory, accountService, settingsService, botLog);
            return await command.ExecuteAsync(userId);
        }

        #endregion Award Poll Approved

        #region Poll Declined Penalty

        public async Task<Result> DeclineUserPoll(ulong userId)
        {
            var command = new DeclineUserPollCommand(scopeFactory, accountService, settingsService, botLog);
            return await command.ExecuteAsync(userId);
        }

        #endregion Poll Declined Penalty

        #region Poll Submitted Penalty

        public async Task<Result> UserSubmittedPollPenalty(ulong userId)
        {
            var command = new UserSubmittedPollPenaltyCommand(scopeFactory, accountService, settingsService, botLog);
            return await command.ExecuteAsync(userId);
        }

        #endregion Poll Submitted Penalty

        #endregion Polls

        #region Lottery

        public async Task<Result> PurchaseLotteryTicket(Account user, int amount)
        {
            var command = new PurchaseLotteryTicketCommand(scopeFactory, botLog);
            return await command.ExecuteAsync(user, amount);
        }

        #region Deposit Lottery Winnings

        public async Task<Result> DepositLotteryWinningsAsync(Account account, int amount)
        {
            var command = new DepositLotteryWinningsCommand(scopeFactory, botLog);
            return await command.ExecuteAsync(account, amount);
        }

        #endregion Deposit Lottery Winnings

        #endregion Lottery

        public async Task<Result> TransferToFitz(ulong userId, int amount, Reason reason)
        {
            var command = new TransferToFitzCommand(scopeFactory, botLog);
            return await command.ExecuteAsync(userId, amount, reason);
        }

        public async Task<Result> TransferBeer(ulong sender, ulong recipient, int amount)
        {
            var command = new TransferBeerCommand(scopeFactory, accountService, botLog);
            return await command.ExecuteAsync(sender, recipient, amount);
        }

        #region Renames

        public async Task PurchaseRenameAsync(Account user, int amount)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                await TransferToFitz(user.Id, amount, Reason.Rename);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to purchase rename.");
            }
        }

        public async Task PurchaseRenameAsync(ulong accountId, int amount)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                Account account = accountService.FindAccount(accountId);
                if (account == null)
                {
                    Log.Error($"Account not found. {accountId}");
                    return;
                }

                await this.PurchaseRenameAsync(account, amount);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to purchase rename.");
            }
        }

        #endregion Renames

        public int GetBalance(ulong userId)
        {
            var query = new GetBalanceQuery(scopeFactory);
            return query.Execute(userId);
        }

        public List<Account> GetTopBeerBalances(int limit = 10)
        {
            var query = new GetTopBeerBalancesQuery(scopeFactory);
            return query.Execute(limit);
        }

        public List<Transaction> GetTransactions(int take)
        {
            var query = new GetTransactionsQuery(scopeFactory);
            return query.Execute(take);
        }

        public List<Transaction> GetTransactions(ulong userId)
        {
            var query = new GetTransactionsQuery(scopeFactory);
            return query.Execute(userId);
        }
    }
}