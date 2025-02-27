using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fitz.Features.Accounts.Domain;
using Fitz.Features.Bank.Models;
using Fitz.Features.Bank.GetBalance.Persistance;

namespace Fitz.Features.Bank.GetBalance.Domain
{
    public class GetBalanceService
    {
        private readonly IGetBalanceRepository _repository;
        private readonly IAccountRepository _accountRepository;

        public GetBalanceService(IGetBalanceRepository repository, IAccountRepository accountRepository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        }

        public async Task<(int Balance, IEnumerable<Transaction> Transactions)> GetBalanceAsync(GetBalanceCommand command)
        {
            var account = await _accountRepository.GetAccountAsync(command.UserId);
            
            if (account == null)
                throw new InvalidOperationException($"Account not found for user {command.UserId}");

            if (account.Deactivated)
                throw new InvalidOperationException($"Account for user {command.UserId} is not active");

            var transactions = command.IncludeTransactions 
                ? await _repository.GetTransactionsAsync(command.UserId, command.TransactionCount) 
                : new List<Transaction>();

            return (account.Beer, transactions);
        }

        public async Task<IEnumerable<(ulong UserId, string Username, int Balance)>> GetTopBalancesAsync(int count = 10)
        {
            if (count <= 0)
                throw new ArgumentException("Count must be greater than 0", nameof(count));

            return await _repository.GetTopBalancesAsync(count);
        }
    }
} 