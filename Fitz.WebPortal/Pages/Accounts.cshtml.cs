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
    public class AccountsModel : PageModel
    {
        private readonly BotContext _context;
        private const int PageSize = 20;

        public AccountsModel(BotContext context)
        {
            _context = context;
        }

        public List<Account> Accounts { get; set; } = new List<Account>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
        {
            CurrentPage = pageNumber < 1 ? 1 : pageNumber;

            // Get total count for pagination
            var totalAccounts = await _context.Accounts.CountAsync();
            TotalPages = (int)Math.Ceiling(totalAccounts / (double)PageSize);

            // Get accounts for current page
            Accounts = await _context.Accounts
                .OrderByDescending(a => a.Beer)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAddBeerAsync(ulong userId, int amount, string reason)
        {
            if (amount <= 0)
            {
                TempData["ErrorMessage"] = "Amount must be greater than 0.";
                return RedirectToPage();
            }

            // Get the account
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == userId);
            if (account == null)
            {
                TempData["ErrorMessage"] = "Account not found.";
                return RedirectToPage();
            }

            // Update the account balance
            account.Beer += amount;
            account.LifetimeBeer += amount;

            // Create a transaction record
            var transaction = new Transaction
            {
                SenderId = 0, // System/Admin
                RecipientId = userId,
                Amount = amount,
                Timestamp = DateTime.UtcNow,
                Reason = reason,
                Type = 1 // Reward type
            };

            // Save changes
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Successfully added {amount} beer to {account.Username}.";
            return RedirectToPage();
        }
    }
} 