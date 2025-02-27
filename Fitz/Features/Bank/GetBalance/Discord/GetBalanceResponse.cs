using System.Collections.Generic;
using Fitz.Features.Bank.Models;

namespace Fitz.Features.Bank.GetBalance.Discord
{
    public class GetBalanceResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int Balance { get; set; }
        public int LifetimeBalance { get; set; }
        public IEnumerable<Transaction> Transactions { get; set; }
    }
} 