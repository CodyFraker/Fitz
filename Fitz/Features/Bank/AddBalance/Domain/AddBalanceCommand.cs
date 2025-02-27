using System;
using Fitz.Features.Bank.Models;

namespace Fitz.Features.Bank.AddBalance.Domain
{
    public class AddBalanceCommand
    {
        public ulong RecipientId { get; }
        public ulong SenderId { get; }
        public int Amount { get; }
        public TransactionReason Reason { get; }
        public bool UpdateLifetimeBalance { get; }

        public AddBalanceCommand(
            ulong recipientId, 
            ulong senderId, 
            int amount, 
            TransactionReason reason,
            bool updateLifetimeBalance = true)
        {
            if (recipientId == 0)
                throw new ArgumentException("Recipient ID cannot be 0", nameof(recipientId));

            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than 0", nameof(amount));

            RecipientId = recipientId;
            SenderId = senderId;
            Amount = amount;
            Reason = reason;
            UpdateLifetimeBalance = updateLifetimeBalance;
        }
    }
}
