using Fitz.Core.Contexts;
using Fitz.Core.Models;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Features.Lottery.Models;
using Fitz.Variables;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Fitz.Features.Lottery
{
    public sealed class LotteryService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private AccountService accountService;
        private BankService bankService;
        private const int MAX_TICKETS = 36;

        public LotteryService(IServiceScopeFactory scopeFactory, AccountService accountService, BankService bankService)
        {
            this.scopeFactory = scopeFactory;
            this.accountService = accountService;
            this.bankService = bankService;
        }

        /// <summary>
        /// Get the most current lottery.
        /// </summary>
        /// <returns>Drawing</returns>
        public Models.Lottery GetCurrentDrawing()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return db.Drawing.Where((x) => x.CurrentLottery == true).FirstOrDefault();
        }

        /// <summary>
        /// Starts a new lottery.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="pool"></param>
        /// <returns></returns>
        public async Task StartNewLotteryAsync(DateTime startDate, DateTime endDate, int pool = 0)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            if (startDate == null)
            {
                startDate = DateTime.UtcNow;
            }
            // Default End date to 7 days from now.
            if (endDate == null)
            {
                endDate = DateTime.UtcNow.AddDays(7);
            }

            // Go get account settings, see who has subscribe to lottery true, buy tickets if over safe balance

            Models.Lottery drawing = new Models.Lottery()
            {
                StartDate = startDate,
                EndDate = endDate,
                Pool = pool,
                WinningTicket = null,
                CurrentLottery = true,
            };
            db.Add(drawing);
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Ends the current lottery.
        /// </summary>
        /// <param name="rollover">If the prizepool should rollover into the next one.</param>
        /// <returns></returns>
        public async Task EndLotteryAsync(Models.Lottery currentLottery)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            currentLottery.CurrentLottery = false;
            db.Update(currentLottery);
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Decide the winners of the current lottery.
        /// </summary>
        /// <param name="drawing"></param>
        /// <returns>List of Winners.</returns>
        public async Task<List<Winners>> DecideWinners(Models.Lottery drawing)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            // Get all tickets from DB for this current lottery.
            List<Ticket> tickets = db.Ticket.Where((x) => x.Drawing == drawing.Id).ToList();
            List<Winners> winners = new List<Winners>();

            int winningTicket = 0;

            // For every ticket we have in the current lottery, generate a winning ticket number.
            // After X amount of tickets, we will have decided a winning ticket number.
            foreach (Ticket ticket in tickets)
            {
                winningTicket = GenerateTicketNumber();
            }
            // Save winning ticket information to the lottery.
            drawing.WinningTicket = winningTicket;
            db.Update(drawing);
            await db.SaveChangesAsync();

            // Check to see if any tickets in the current lottery match our winning ticket number.
            List<Ticket> winningTickets = tickets.Where(tickets => tickets.Number == winningTicket).ToList();

            if (winningTickets.Count > 0)
            {
                // Get all accounts who has a winning ticket
                List<Account> accounts = new List<Account>();
                foreach (Ticket ticket in winningTickets)
                {
                    Account account = db.Accounts.Where((x) => x.Id == ticket.AccountId).FirstOrDefault();
                    accounts.Add(account);
                }
                int payout = (drawing.Pool ?? 0) / accounts.Count;

                foreach (Account account in accounts)
                {
                    // stupid and bad code. I'm sorry.
                    Winners winner = new Winners()
                    {
                        Drawing = drawing.Id,
                        AccountId = account.Id,
                        Payout = payout,
                        WinningTicketId = drawing.WinningTicket ?? 0,
                        Timestamp = DateTime.UtcNow,
                    };
                    db.Add(winner);
                    await db.SaveChangesAsync();

                    // Pay users their winnings.
                    await this.bankService.DepositLotteryWinningsAsync(account, payout);
                    winners.Add(winner);
                }
                return winners;
            }
            return winners;
        }

        public async Task UpdateCurrentLottery(DateTime endDate)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Models.Lottery drawing = this.GetCurrentDrawing();
            drawing.EndDate = endDate;
            db.Update(drawing);
            await db.SaveChangesAsync();
        }

        public async Task<Result> SetLotteryPrizePoolAsync(int pool)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                Models.Lottery drawing = this.GetCurrentDrawing();
                if (drawing == null)
                {
                    return new Result(false, "There is no active lottery.", null);
                }

                drawing.Pool = pool;
                db.Update(drawing);
                await db.SaveChangesAsync();
                return new Result(true, $"Set prize pool to {pool}.", null);
            }
            catch (Exception ex)
            {
                return new Result(false, "Failed to set prize pool.", ex);
            }
        }

        public async Task CreateTicket(Account account, int totalTickets)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            // Get account tickets for this current lottery.
            List<Ticket> accountTickets = this.GetUserTickets(account);
            if (accountTickets.Count >= MAX_TICKETS)
            {
                // User already has max amount of tickets. No more can be added to this current lottery.
                return;
            }

            Models.Lottery drawing = this.GetCurrentDrawing();
            for (int i = 0; i < totalTickets; i++)
            {
                if (accountTickets.Count >= MAX_TICKETS)
                {
                    //User already has max amount of tickets.No more can be added to this current lottery.
                    return;
                }
                int ticketNumber = GenerateTicketNumber();
                if (!accountTickets.Any((x) => x.Number == ticketNumber))
                {
                    Ticket newTicket = new Ticket()
                    {
                        Drawing = drawing.Id,
                        Number = ticketNumber,
                        AccountId = account.Id,
                        Timestamp = DateTime.UtcNow,
                    };
                    accountTickets.Add(newTicket);
                    db.Add(newTicket);
                    await db.SaveChangesAsync();
                }
                else
                {
                    await GenerateTicketForFitz();
                }
            }
        }

        public async Task GenerateTicketForFitz()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            // Get account tickets for this current lottery.
            List<Ticket> accountTickets = this.GetUserTickets(accountService.FindAccount(Users.Fitz));

            Models.Lottery drawing = this.GetCurrentDrawing();
            int ticketNumber = GenerateTicketNumber();
            if (!accountTickets.Any((x) => x.Number == ticketNumber))
            {
                Ticket newTicket = new Ticket()
                {
                    Drawing = drawing.Id,
                    Number = ticketNumber,
                    AccountId = Users.Fitz,
                    Timestamp = DateTime.UtcNow,
                };
                accountTickets.Add(newTicket);
                db.Add(newTicket);
                await db.SaveChangesAsync();
            }
        }

        public async Task<Result> BuyTicketsForFitz(int totalTickets)
        {
            try
            {
                Account fitz = accountService.FindAccount(Users.Fitz);
                if (fitz.Beer < totalTickets)
                {
                    return new Result(false, "Fitz does not have enough beer to buy tickets.", fitz);
                }
                else
                {
                    for (int i = 0; i < totalTickets; i++)
                    {
                        await GenerateTicketForFitz();
                    }
                    return new Result(true, $"Successfully bought {totalTickets} tickets for Fitz.", fitz);
                }
            }
            catch (Exception ex)
            {
                return new Result(false, "Failed to buy tickets for Fitz.", ex);
            }
        }

        public async Task<int> GetRemainingHoursUntilNextDrawing()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Models.Lottery drawing = this.GetCurrentDrawing();
            return (int)(drawing.EndDate - DateTime.UtcNow).TotalHours;
        }

        public async Task<int> GetTotalTickets()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Models.Lottery drawing = this.GetCurrentDrawing();
            int totalTickets = db.Ticket.Where((x) => x.Drawing == drawing.Id).Count();
            return totalTickets;
        }

        public async Task<int> GetTotalLotteryParticipant()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Models.Lottery drawing = this.GetCurrentDrawing();
            List<Ticket> tickets = db.Ticket.Where((x) => x.Drawing == drawing.Id).ToList();
            List<ulong> users = tickets.Select((x) => x.AccountId).Distinct().ToList();
            return users.Count;
        }

        public async Task<int> GetCurrentDrawingId()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Models.Lottery drawing = this.GetCurrentDrawing();
            return drawing.Id;
        }

        public async Task<int> GetCurrentPrizePool()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Models.Lottery drawing = this.GetCurrentDrawing();
            return drawing.Pool ?? 0;
        }

        public async Task AddToPool(int amount)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Models.Lottery drawing = this.GetCurrentDrawing();
            drawing.Pool += amount;
            db.Update(drawing);
            await db.SaveChangesAsync();
        }

        public List<Ticket> GetUserTickets(Account account)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Models.Lottery drawing = this.GetCurrentDrawing();
            return db.Ticket.Where((x) => x.AccountId == account.Id && x.Drawing == drawing.Id).ToList();
        }

        public int GetLastWinningTicket()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Models.Lottery drawing = db.Drawing.Where((x) => x.CurrentLottery == false).OrderByDescending((x) => x.EndDate).FirstOrDefault();

            if (drawing == null)
            {
                return 0;
            }
            return drawing.WinningTicket ?? 0;
        }

        public List<Winners> GetLastLotteryWinners()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Models.Lottery drawing = db.Drawing.Where((x) => x.CurrentLottery == false).OrderByDescending((x) => x.EndDate).FirstOrDefault();
            return db.Winners.Where((x) => x.Drawing == drawing.Id).ToList();
        }

        public List<Account> GetLastLotteryWinnerAccounts()
        {
            List<Account> accounts = new List<Account>();
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            // Just use the method to get last drawing instead of calling it everywhere else.
            Models.Lottery drawing = db.Drawing.Where((x) => x.CurrentLottery == false).OrderByDescending((x) => x.EndDate).FirstOrDefault();

            if (drawing == null)
            {
                return accounts;
            }

            List<Winners> winners = db.Winners.Where((x) => x.Drawing == drawing.Id).ToList();
            foreach (Winners winner in winners)
            {
                Account account = db.Accounts.Where((x) => x.Id == winner.AccountId).FirstOrDefault();
                if (accounts.Contains(account) == false)
                {
                    accounts.Add(account);
                }
            }
            return accounts;
        }

        private int GenerateTicketNumber()
        {
            int ticket = 0;

            // Make sure ticket is between 0 and 1000
            while (ticket <= 0 || ticket > 1000)
            {
                DateTime UTCNow = DateTime.UtcNow;
                int month = UTCNow.Month;
                int day = UTCNow.Day;
                int hour = UTCNow.Hour;
                int min = UTCNow.Minute;
                int sec = UTCNow.Second;
                Random salt = new Random();

                ticket = salt.Next(20, 1000);
                ticket = ticket + salt.Next(month, 100);
                ticket = ticket - salt.Next(min, 99);
                ticket = ticket - salt.Next(hour, 24);
                ticket = ticket + salt.Next(sec, 600);
                ticket = ticket + salt.Next(day, 80);
                ticket = ticket + salt.Next(1, 350);
            }
            return ticket;
        }
    }
}