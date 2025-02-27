using System;
using System.Threading.Tasks;
using Fitz.Core.Models;
using Fitz.Features.Accounts.Domain;
using Fitz.Features.Bank.AddBalance.Persistance;
using Fitz.Features.Bank.Models;

namespace Fitz.Features.Bank.AddBalance.Domain
{
    public class AddBalanceService
    {
        private readonly IAddBalanceRepository _repository;
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;

        public AddBalanceService(
            IAddBalanceRepository repository,
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        }

        public async Task<Result> AddBalanceAsync(AddBalanceCommand command)
        {
            try
            {
                // Get recipient account
                var account = await _accountRepository.GetAccountAsync(command.RecipientId);
                if (account == null)
                    return new Result(false, $"Account not found for user {command.RecipientId}", null);

                if (!account.IsActive)
                    return new Result(false, $"Account for user {command.RecipientId} is deactivated", null);

                // Update balance
                account.Balance += command.Amount;
                
                // Update lifetime balance if needed
                if (command.UpdateLifetimeBalance)
                    account.LifetimeBalance += command.Amount;

                // Save changes
                await _accountRepository.UpdateAccountAsync(account);

                // Log transaction
                await _repository.LogTransactionAsync(command.SenderId, command.RecipientId, command.Amount, command.Reason);

                return new Result(true, $"Added {command.Amount} beer to account", account);
            }
            catch (Exception ex)
            {
                return new Result(false, $"Failed to add balance: {ex.Message}", null);
            }
        }

        public async Task<Result> DeductBalanceAsync(DeductBalanceCommand command)
        {
            try
            {
                // Get user account
                var account = await _accountRepository.GetAccountAsync(command.UserId);
                if (account == null)
                    return new Result(false, $"Account not found for user {command.UserId}", null);

                if (!account.IsActive)
                    return new Result(false, $"Account for user {command.UserId} is deactivated", null);

                // Check if user has enough balance
                if (account.Balance < command.Amount)
                    return new Result(false, $"User does not have enough beer. Current balance: {account.Balance}", account);

                // Update balance
                account.Balance -= command.Amount;

                // Save changes
                await _accountRepository.UpdateAccountAsync(account);

                // Log transaction
                await _repository.LogTransactionAsync(command.UserId, command.UserId, -command.Amount, command.Reason);

                return new Result(true, $"Deducted {command.Amount} beer from account", account);
            }
            catch (Exception ex)
            {
                return new Result(false, $"Failed to deduct balance: {ex.Message}", null);
            }
        }

        public async Task<Result> AwardAccountCreationBonusAsync(ulong userId, int amount)
        {
            var command = new AddBalanceCommand(
                recipientId: userId,
                senderId: userId,
                amount: amount,
                reason: Models.TransactionReason.AccountCreationBonus,
                updateLifetimeBalance: true);

            return await AddBalanceAsync(command);
        }

        public async Task<Result> AwardHappyHourAsync(ulong userId, int amount)
        {
            var command = new AddBalanceCommand(
                recipientId: userId,
                senderId: userId,
                amount: amount,
                reason: Models.TransactionReason.HappyHour,
                updateLifetimeBalance: true);

            return await AddBalanceAsync(command);
        }

        public async Task<Result> TransferBalanceAsync(ulong senderId, ulong recipientId, int amount)
        {
            try
            {
                // Deduct from sender
                var deductCommand = new DeductBalanceCommand(senderId, amount, Models.TransactionReason.Donated);
                var deductResult = await DeductBalanceAsync(deductCommand);

                if (!deductResult.Success)
                    return deductResult;

                // Add to recipient
                var addCommand = new AddBalanceCommand(
                    recipientId: recipientId,
                    senderId: senderId,
                    amount: amount,
                    reason: Models.TransactionReason.Donated,
                    updateLifetimeBalance: true);

                return await AddBalanceAsync(addCommand);
            }
            catch (Exception ex)
            {
                return new Result(false, $"Failed to transfer balance: {ex.Message}", null);
            }
        }

        public async Task<Result> AddBalance(AddBalanceCommand command)
        {
            try
            {
                // Get recipient account
                var recipient = await _accountRepository.GetAccountByUserId(command.RecipientId);
                if (recipient == null)
                {
                    // Create new account if it doesn't exist
                    recipient = new Account
                    {
                        UserId = command.RecipientId,
                        Balance = 0,
                        LifetimeBalance = 0,
                        LastUpdated = DateTime.UtcNow,
                        IsActive = true
                    };
                }

                // Update recipient balance
                recipient.Balance += command.Amount;
                if (command.UpdateLifetimeBalance)
                {
                    recipient.LifetimeBalance += command.Amount;
                }
                recipient.LastUpdated = DateTime.UtcNow;

                // Save account changes
                await _accountRepository.SaveAccount(recipient);

                // Record transaction
                var transaction = new Transaction
                {
                    SenderId = command.SenderId,
                    RecipientId = command.RecipientId,
                    Amount = command.Amount,
                    Reason = command.Reason.ToString(),
                    Timestamp = DateTime.UtcNow,
                    Type = TransactionType.Transfer
                };

                await _transactionRepository.SaveTransaction(transaction);

                return new Result(true, $"Successfully added {command.Amount} to account {command.RecipientId}", recipient);
            }
            catch (Exception ex)
            {
                return new Result(false, $"Failed to add balance: {ex.Message}", null);
            }
        }
    }

    public interface IAccountRepository
    {
        Task<Account> GetAccountByUserId(ulong userId);
        Task SaveAccount(Account account);
        Task<Account> GetAccountAsync(ulong userId);
        Task UpdateAccountAsync(Account account);
    }

    public interface ITransactionRepository
    {
        Task SaveTransaction(Transaction transaction);
    }
} 