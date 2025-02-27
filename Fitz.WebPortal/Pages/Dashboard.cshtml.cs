using Fitz.WebPortal.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.WebPortal.Pages
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly BotContext _context;

        public DashboardModel(BotContext context)
        {
            _context = context;
        }

        public int TotalAccounts { get; set; }
        public int CurrentLotteryPool { get; set; }
        public int TotalTransactions { get; set; }
        public List<TransactionViewModel> RecentTransactions { get; set; } = new List<TransactionViewModel>();
        public List<Account> TopAccounts { get; set; } = new List<Account>();

        public async Task<IActionResult> OnGetAsync()
        {
            // Get total accounts
            TotalAccounts = await _context.Accounts.CountAsync(a => !a.Deactivated);

            // Get current lottery pool
            var currentLottery = await _context.Lotteries.FirstOrDefaultAsync(l => l.CurrentLottery);
            CurrentLotteryPool = currentLottery?.Pool ?? 0;

            // Get total transactions
            TotalTransactions = await _context.Transactions.CountAsync();

            // Get recent transactions
            var recentTransactions = await _context.Transactions
                .OrderByDescending(t => t.Timestamp)
                .Take(10)
                .ToListAsync();

            // Get all accounts for username lookup
            var accounts = await _context.Accounts.ToListAsync();
            var accountLookup = accounts.ToDictionary(a => a.Id, a => a.Username);

            // Map transactions to view model
            RecentTransactions = recentTransactions.Select(t => new TransactionViewModel
            {
                Id = t.Id,
                SenderId = t.SenderId,
                RecipientId = t.RecipientId,
                SenderName = accountLookup.TryGetValue(t.SenderId, out var senderName) ? senderName : "System",
                RecipientName = accountLookup.TryGetValue(t.RecipientId, out var recipientName) ? recipientName : "Unknown",
                Amount = t.Amount,
                Timestamp = t.Timestamp,
                Reason = t.Reason
            }).ToList();

            // Get top accounts by beer
            TopAccounts = await _context.Accounts
                .Where(a => !a.Deactivated)
                .OrderByDescending(a => a.Beer)
                .Take(10)
                .ToListAsync();

            return Page();
        }

        public class TransactionViewModel
        {
            public int Id { get; set; }
            public ulong SenderId { get; set; }
            public ulong RecipientId { get; set; }
            public string SenderName { get; set; }
            public string RecipientName { get; set; }
            public int Amount { get; set; }
            public DateTime Timestamp { get; set; }
            public string Reason { get; set; }
        }
    }
} 