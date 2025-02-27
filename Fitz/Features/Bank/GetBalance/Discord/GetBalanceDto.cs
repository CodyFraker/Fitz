namespace Fitz.Features.Bank.GetBalance.Discord
{
    public class GetBalanceDto
    {
        public ulong UserId { get; set; }
        public bool IncludeTransactions { get; set; }
        public int TransactionCount { get; set; }
    }
} 