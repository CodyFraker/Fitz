using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fitz.Features.Bank.Models;
using Fitz.Features.Accounts.Models;
using Fitz.Core.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Features.Bank.GetBalance.Persistance
{
    public class GetBalanceRepository : IGetBalanceRepository
    {
        private readonly BotContext _context;

        public GetBalanceRepository(BotContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsAsync(ulong userId, int count)
        {
            return await _context.Set<Transaction>()
                .Where(t => t.SenderId == userId || t.RecipientId == userId)
                .OrderByDescending(t => t.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<(ulong UserId, string Username, int Balance)>> GetTopBalancesAsync(int count)
        {
            var results = await _context.Accounts
                .Where(a => !a.Deactivated)
                .OrderByDescending(a => a.Beer)
                .Take(count)
                .Select(a => new { UserId = a.Id, Username = a.Username, Balance = a.Beer })
                .ToListAsync();

            return results.Select(a => (a.UserId, a.Username ?? "Unknown", a.Balance));
        }
    }
} 