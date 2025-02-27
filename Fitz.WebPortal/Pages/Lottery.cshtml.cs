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
    public class LotteryModel : PageModel
    {
        private readonly BotContext _context;

        public LotteryModel(BotContext context)
        {
            _context = context;
        }

        public Lottery CurrentLottery { get; set; }
        public List<Lottery> PastLotteries { get; set; } = new List<Lottery>();
        public int TotalTickets { get; set; }
        public string TimeRemaining { get; set; }
        
        public class WinnerInfo
        {
            public int TicketId { get; set; }
            public string Username { get; set; }
            public int PrizeAmount { get; set; }
        }
        
        public List<WinnerInfo> Winners { get; set; } = new List<WinnerInfo>();

        public async Task<IActionResult> OnGetAsync()
        {
            // Get current lottery (the one with the latest end date that hasn't ended yet)
            CurrentLottery = await _context.Lotteries
                .Where(l => l.EndDate > DateTime.UtcNow)
                .OrderBy(l => l.EndDate)
                .FirstOrDefaultAsync();

            if (CurrentLottery != null)
            {
                // Calculate time remaining
                var timeSpan = CurrentLottery.EndDate - DateTime.UtcNow;
                if (timeSpan.TotalDays >= 1)
                {
                    TimeRemaining = $"{(int)timeSpan.TotalDays} days, {timeSpan.Hours} hours";
                }
                else if (timeSpan.TotalHours >= 1)
                {
                    TimeRemaining = $"{timeSpan.Hours} hours, {timeSpan.Minutes} minutes";
                }
                else
                {
                    TimeRemaining = $"{timeSpan.Minutes} minutes, {timeSpan.Seconds} seconds";
                }

                // Get total tickets for current lottery
                TotalTickets = await _context.LotteryTickets
                    .CountAsync(t => t.LotteryId == CurrentLottery.Id);
            }

            // Get past lotteries (ended lotteries)
            PastLotteries = await _context.Lotteries
                .Where(l => l.EndDate <= DateTime.UtcNow)
                .OrderByDescending(l => l.EndDate)
                .Take(10)
                .ToListAsync();

            // Get winner information for past lotteries
            var winningTicketIds = PastLotteries
                .Where(l => l.WinningTicketId.HasValue)
                .Select(l => l.WinningTicketId.Value)
                .ToList();

            if (winningTicketIds.Any())
            {
                var winningTickets = await _context.LotteryTickets
                    .Where(t => winningTicketIds.Contains(t.Id))
                    .ToListAsync();

                var userIds = winningTickets.Select(t => t.UserId).Distinct().ToList();
                var accounts = await _context.Accounts
                    .Where(a => userIds.Contains(a.Id))
                    .ToDictionaryAsync(a => a.Id, a => a.Username);

                foreach (var lottery in PastLotteries.Where(l => l.WinningTicketId.HasValue))
                {
                    var ticket = winningTickets.FirstOrDefault(t => t.Id == lottery.WinningTicketId.Value);
                    if (ticket != null && accounts.TryGetValue(ticket.UserId, out var username))
                    {
                        Winners.Add(new WinnerInfo
                        {
                            TicketId = ticket.Id,
                            Username = username,
                            PrizeAmount = lottery.Pool
                        });
                    }
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCreateLotteryAsync(int durationDays, int initialPool)
        {
            // Check if there's already an active lottery
            var existingLottery = await _context.Lotteries
                .AnyAsync(l => l.EndDate > DateTime.UtcNow);

            if (existingLottery)
            {
                TempData["ErrorMessage"] = "There is already an active lottery. End the current lottery before creating a new one.";
                return RedirectToPage();
            }

            if (durationDays <= 0)
            {
                TempData["ErrorMessage"] = "Duration must be at least 1 day.";
                return RedirectToPage();
            }

            if (initialPool < 0)
            {
                TempData["ErrorMessage"] = "Initial pool cannot be negative.";
                return RedirectToPage();
            }

            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(durationDays);

            var newLottery = new Lottery
            {
                StartDate = startDate,
                EndDate = endDate,
                Pool = initialPool,
                CurrentLottery = true,
                WinningTicketId = null
            };

            _context.Lotteries.Add(newLottery);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"New lottery created successfully! It will run until {endDate.ToString("MM/dd/yyyy HH:mm")} UTC.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdatePoolAsync(int newPoolAmount)
        {
            var currentLottery = await _context.Lotteries
                .FirstOrDefaultAsync(l => l.EndDate > DateTime.UtcNow);

            if (currentLottery == null)
            {
                TempData["ErrorMessage"] = "No active lottery found.";
                return RedirectToPage();
            }

            if (newPoolAmount < 0)
            {
                TempData["ErrorMessage"] = "Pool amount cannot be negative.";
                return RedirectToPage();
            }

            currentLottery.Pool = newPoolAmount;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Lottery pool updated to {newPoolAmount}.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEndLotteryAsync()
        {
            var currentLottery = await _context.Lotteries
                .FirstOrDefaultAsync(l => l.EndDate > DateTime.UtcNow);

            if (currentLottery == null)
            {
                TempData["ErrorMessage"] = "No active lottery found.";
                return RedirectToPage();
            }

            // Get all tickets for this lottery
            var tickets = await _context.LotteryTickets
                .Where(t => t.LotteryId == currentLottery.Id)
                .ToListAsync();

            if (!tickets.Any())
            {
                TempData["ErrorMessage"] = "Cannot end lottery: No tickets have been purchased.";
                return RedirectToPage();
            }

            // Select a random ticket as the winner
            var random = new Random();
            var winningIndex = random.Next(tickets.Count);
            var winningTicket = tickets[winningIndex];

            // Update the lottery with the winning ticket and set end date to now
            currentLottery.WinningTicketId = winningTicket.Id;
            currentLottery.EndDate = DateTime.UtcNow;
            currentLottery.CurrentLottery = false;

            // Get the winner's account
            var winnerAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == winningTicket.UserId);

            if (winnerAccount != null)
            {
                // Add the prize to the winner's account
                winnerAccount.Beer += currentLottery.Pool;
                winnerAccount.LifetimeBeer += currentLottery.Pool;

                // Create a transaction record
                var transaction = new Transaction
                {
                    SenderId = 0, // System
                    RecipientId = winnerAccount.Id,
                    Amount = currentLottery.Pool,
                    Timestamp = DateTime.UtcNow,
                    Reason = $"Lottery #{currentLottery.Id} prize",
                    Type = 3 // Lottery type
                };

                _context.Transactions.Add(transaction);
            }

            await _context.SaveChangesAsync();

            var winnerName = winnerAccount?.Username ?? "Unknown";
            TempData["SuccessMessage"] = $"Lottery ended successfully! Winner: {winnerName}, Prize: {currentLottery.Pool}";
            return RedirectToPage();
        }
    }
} 