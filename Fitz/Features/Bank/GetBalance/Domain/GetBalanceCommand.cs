using System;

namespace Fitz.Features.Bank.GetBalance.Domain
{
    public class GetBalanceCommand
    {
        public ulong UserId { get; }
        public bool IncludeTransactions { get; }
        public int TransactionCount { get; }

        public GetBalanceCommand(ulong userId, bool includeTransactions = true, int transactionCount = 10)
        {
            if (userId == 0)
                throw new ArgumentException("User ID cannot be 0", nameof(userId));

            if (transactionCount < 0)
                throw new ArgumentException("Transaction count cannot be negative", nameof(transactionCount));

            UserId = userId;
            IncludeTransactions = includeTransactions;
            TransactionCount = transactionCount;
        }
    }
}
