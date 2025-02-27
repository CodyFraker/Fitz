using System;
using Fitz.Features.Bank.Models;

namespace Fitz.Features.Bank.AddBalance.Domain
{
    public class DeductBalanceCommand
    {
        public ulong UserId { get; }
        public int Amount { get; }
        public TransactionReason Reason { get; }

        public DeductBalanceCommand(ulong userId, int amount, TransactionReason reason)
        {
            if (userId == 0)
                throw new ArgumentException("User ID cannot be 0", nameof(userId));

            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than 0", nameof(amount));

            UserId = userId;
            Amount = amount;
            Reason = reason;
        }
    }
} 