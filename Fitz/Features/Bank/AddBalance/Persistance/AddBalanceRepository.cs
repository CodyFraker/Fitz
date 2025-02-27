using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Features.Bank.AddBalance.Domain;
using Fitz.Features.Bank.Models;
using Fitz.Variables.Emojis;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Bank.AddBalance.Persistance
{
    public class AddBalanceRepository : IAccountRepository, ITransactionRepository, IAddBalanceRepository
    {
        private readonly BotContext _dbContext;
        private readonly BotLog _botLog;

        public AddBalanceRepository(BotContext dbContext, BotLog botLog)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _botLog = botLog ?? throw new ArgumentNullException(nameof(botLog));
        }

        public async Task<Account> GetAccountByUserId(ulong userId)
        {
            try
            {
                return await _dbContext.Set<Account>()
                    .FirstOrDefaultAsync(a => a.UserId == userId);
            }
            catch (Exception ex)
            {
                _botLog.Error($"Error getting account for user {userId}: {ex.Message}");
                return null;
            }
        }

        public async Task<Account> GetAccountAsync(ulong userId)
        {
            try
            {
                return await _dbContext.Set<Account>()
                    .FirstOrDefaultAsync(a => a.UserId == userId);
            }
            catch (Exception ex)
            {
                _botLog.Error($"Error getting account for user {userId}: {ex.Message}");
                return null;
            }
        }

        public async Task UpdateAccountAsync(Account account)
        {
            try
            {
                var existingAccount = await GetAccountByUserId(account.UserId);
                if (existingAccount == null)
                {
                    throw new InvalidOperationException($"Account not found for user {account.UserId}");
                }
                
                _dbContext.Entry(existingAccount).CurrentValues.SetValues(account);
                _botLog.Information(LogConsoleSettings.BankLog, BankEmojis.Transaction, $"Account updated for user {account.UserId}");
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _botLog.Error($"Error updating account for user {account.UserId}: {ex.Message}");
                throw;
            }
        }

        public async Task SaveAccount(Account account)
        {
            try
            {
                var existingAccount = await GetAccountByUserId(account.UserId);
                if (existingAccount == null)
                {
                    await _dbContext.Set<Account>().AddAsync(account);
                    _botLog.Information(LogConsoleSettings.BankLog, BankEmojis.Transaction, $"New account created for user {account.UserId}");
                }
                else
                {
                    _dbContext.Entry(existingAccount).CurrentValues.SetValues(account);
                    _botLog.Information(LogConsoleSettings.BankLog, BankEmojis.Transaction, $"Account updated for user {account.UserId}");
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _botLog.Error($"Error saving account for user {account.UserId}: {ex.Message}");
                throw;
            }
        }

        public async Task SaveTransaction(Transaction transaction)
        {
            try
            {
                await _dbContext.Set<Transaction>().AddAsync(transaction);
                await _dbContext.SaveChangesAsync();
                _botLog.Information(LogConsoleSettings.BankLog, BankEmojis.Transaction, 
                    $"Transaction saved: {transaction.SenderId} -> {transaction.RecipientId}, Amount: {transaction.Amount}, Reason: {transaction.Reason}");
            }
            catch (Exception ex)
            {
                _botLog.Error($"Error saving transaction: {transaction.SenderId} -> {transaction.RecipientId}, Amount: {transaction.Amount}, Reason: {transaction.Reason}. Error: {ex.Message}");
                throw;
            }
        }
        
        public async Task LogTransactionAsync(ulong senderId, ulong recipientId, int amount, TransactionReason reason)
        {
            var transaction = new Transaction
            {
                SenderId = senderId,
                RecipientId = recipientId,
                Amount = amount,
                Reason = reason.ToString(),
                Timestamp = DateTime.UtcNow
            };
            
            await SaveTransaction(transaction);
        }
    }
}
