using Fitz.Features.Bank.Models;

namespace Fitz.Features.Bank.AddBalance.Discord
{
    public class AddBalanceDto
    {
        public ulong RecipientId { get; set; }
        public ulong SenderId { get; set; }
        public int Amount { get; set; }
        public TransactionReason Reason { get; set; }
        public bool UpdateLifetimeBalance { get; set; } = true;
    }
}
