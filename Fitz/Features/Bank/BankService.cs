using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank.AddBalance.Domain;
using Fitz.Features.Bank.GetBalance.Domain;
using Fitz.Features.Bank.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Fitz.Features.Bank
{
    // This file provides backward compatibility for the old BankService class
    // after the refactoring to a more domain-driven design approach.
    
    /// <summary>
    /// Service for managing the bank system
    /// </summary>
    public class BankService
    {
        private readonly AddBalanceService _addBalanceService;
        private readonly GetBalanceService _getBalanceService;
        
        public BankService(AddBalanceService addBalanceService, GetBalanceService getBalanceService)
        {
            _addBalanceService = addBalanceService ?? throw new ArgumentNullException(nameof(addBalanceService));
            _getBalanceService = getBalanceService ?? throw new ArgumentNullException(nameof(getBalanceService));
        }
        
        // Add compatibility methods as needed
        
        /// <summary>
        /// Awards a bonus to a user for creating an account
        /// </summary>
        /// <param name="account">The account to award the bonus to</param>
        /// <returns>Task</returns>
        public async Task AwardAccountCreationBonusAsync(Fitz.Features.Accounts.Models.Account account)
        {
            const int ACCOUNT_CREATION_BONUS = 100; // Default bonus amount
            
            var command = new AddBalanceCommand(
                recipientId: account.Id,
                senderId: 0, // System transaction
                amount: ACCOUNT_CREATION_BONUS,
                reason: TransactionReason.AccountCreationBonus,
                updateLifetimeBalance: true // Update lifetime balance for bonuses
            );
            
            await _addBalanceService.AddBalanceAsync(command);
        }
        
        /// <summary>
        /// Purchases a rename for a user
        /// </summary>
        /// <param name="userId">The user ID of the purchaser</param>
        /// <param name="cost">The cost of the rename</param>
        /// <returns>Task</returns>
        public async Task PurchaseRenameAsync(ulong userId, int cost)
        {
            var command = new AddBalanceCommand(
                recipientId: userId,
                senderId: 0, // System transaction
                amount: -cost, // Negative amount for purchase
                reason: TransactionReason.Rename,
                updateLifetimeBalance: false // Don't update lifetime balance for purchases
            );
            
            await _addBalanceService.AddBalanceAsync(command);
        }
        
        /// <summary>
        /// Purchases lottery tickets for a user
        /// </summary>
        /// <param name="account">The account of the purchaser</param>
        /// <param name="ticketCount">The number of tickets to purchase</param>
        /// <returns>Task</returns>
        public async Task PurchaseLotteryTicket(Fitz.Features.Accounts.Models.Account account, int ticketCount)
        {
            var command = new AddBalanceCommand(
                recipientId: account.Id,
                senderId: 0, // System transaction
                amount: -(ticketCount * 10), // Negative amount for purchase
                reason: TransactionReason.Lotto,
                updateLifetimeBalance: false // Don't update lifetime balance for purchases
            );
            
            await _addBalanceService.AddBalanceAsync(command);
        }
        
        /// <summary>
        /// Transfers beer to Fitz
        /// </summary>
        /// <param name="userId">The user ID of the sender</param>
        /// <param name="amount">The amount to transfer</param>
        /// <param name="reason">The reason for the transfer</param>
        /// <returns>Task</returns>
        public async Task TransferToFitz(ulong userId, int amount, TransactionReason reason = TransactionReason.Donated)
        {
            var command = new AddBalanceCommand(
                recipientId: userId,
                senderId: 0, // Fitz/System
                amount: -amount, // Negative amount for transfer out
                reason: reason,
                updateLifetimeBalance: false // Don't update lifetime balance for transfers
            );
            
            await _addBalanceService.AddBalanceAsync(command);
        }
        
        /// <summary>
        /// Gets the top beer balances
        /// </summary>
        /// <param name="count">The number of top balances to retrieve</param>
        /// <returns>A list of user IDs, usernames, and balances</returns>
        public async Task<IEnumerable<(ulong UserId, string Username, int Balance)>> GetTopBeerBalances(int count = 10)
        {
            return await _getBalanceService.GetTopBalancesAsync(count);
        }
        
        /// <summary>
        /// Awards beer for happy hour
        /// </summary>
        /// <param name="userId">The user ID to award beer to</param>
        /// <param name="amount">The amount of beer to award</param>
        /// <returns>Task</returns>
        public async Task AwardHappyHour(ulong userId, int amount)
        {
            var command = new AddBalanceCommand(
                recipientId: userId,
                senderId: 0, // System transaction
                amount: amount, // Positive amount for award
                reason: TransactionReason.HappyHour,
                updateLifetimeBalance: true // Update lifetime balance for awards
            );
            
            await _addBalanceService.AddBalanceAsync(command);
        }

        /// <summary>
        /// Applies a penalty to a user for submitting a poll
        /// </summary>
        /// <param name="accountId">The Discord user ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task UserSubmittedPollPenalty(ulong accountId)
        {
            // Implementation will be added later
            
            await Task.CompletedTask;
        }

        /// <summary>
        /// Awards beer to a user for having their poll approved
        /// </summary>
        /// <param name="accountId">The Discord user ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task AwardPollApproval(ulong accountId)
        {
            // Implementation will be added later
            
            await Task.CompletedTask;
        }

        /// <summary>
        /// Awards beer to a user for voting on a poll
        /// </summary>
        /// <param name="accountId">The Discord user ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task AwardPollVote(ulong accountId)
        {
            // Implementation will be added later
            
            await Task.CompletedTask;
        }

        /// <summary>
        /// Tips beer to a poll creator when someone votes on their poll
        /// </summary>
        /// <param name="accountId">The Discord user ID of the poll creator</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task TipPollCreatorVote(ulong accountId)
        {
            // Implementation will be added later
            
            await Task.CompletedTask;
        }
    }
} 