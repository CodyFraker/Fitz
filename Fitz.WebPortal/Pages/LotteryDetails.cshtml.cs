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
    public class LotteryDetailsModel : PageModel
    {
        private readonly BotContext _context;

        public LotteryDetailsModel(BotContext context)
        {
            _context = context;
        }

        public Lottery Lottery { get; set; }
        public List<LotteryTicket> Tickets { get; set; } = new List<LotteryTicket>();
        public Dictionary<ulong, string> UserNames { get; set; } = new Dictionary<ulong, string>();
        public int TotalTickets => Tickets.Count;
        public string WinnerUsername { get; set; } = "Unknown";

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Get the lottery by ID
            Lottery = await _context.Lotteries
                .FirstOrDefaultAsync(l => l.Id == id);

            if (Lottery == null)
            {
                return Page();
            }

            // Get all tickets for this lottery
            Tickets = await _context.LotteryTickets
                .Where(t => t.LotteryId == Lottery.Id)
                .OrderBy(t => t.PurchaseDate)
                .ToListAsync();

            // Get usernames for all ticket holders
            if (Tickets.Any())
            {
                var userIds = Tickets.Select(t => t.UserId).Distinct().ToList();
                var accounts = await _context.Accounts
                    .Where(a => userIds.Contains(a.Id))
                    .ToDictionaryAsync(a => a.Id, a => a.Username);

                UserNames = accounts;

                // Get winner username if there is a winning ticket
                if (Lottery.WinningTicketId.HasValue)
                {
                    var winningTicket = Tickets.FirstOrDefault(t => t.Id == Lottery.WinningTicketId.Value);
                    if (winningTicket != null && UserNames.TryGetValue(winningTicket.UserId, out var username))
                    {
                        WinnerUsername = username;
                    }
                }
            }

            return Page();
        }
    }
} 