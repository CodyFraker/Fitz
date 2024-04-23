using Fitz.Core.Contexts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Lottery.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Lottery
{
    public sealed class LotteryService
    {
        private readonly IServiceScopeFactory scopeFactory;

        public LotteryService(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }

        public async Task<Drawing> GetCurrentLottery()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Drawing drawing = db.Drawing.Where((x) => x.CurrentLottery == true).FirstOrDefault();

            if (drawing == null)
            {
                await StartLottery();
                return await GetCurrentLottery();
            }

            if (drawing.EndDate < DateTime.UtcNow)
            {
                await EndLottery();
                return await GetCurrentLottery();
            }
            else
            {
                return drawing;
            }
        }

        public async Task StartLottery()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Drawing drawing = new Drawing()
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddHours(24),
                Pool = 36,
                WinningTicket = null,
                WinningUserId = null,
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
        public async Task EndLottery(bool rollover = false)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Drawing drawing = await GetCurrentLottery();
            drawing.CurrentLottery = false;
            await DecideWinner(drawing);
            db.Update(drawing);
            await db.SaveChangesAsync();
        }

        private async Task<Account> DecideWinner(Drawing drawing)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            List<Ticket> tickets = db.Ticket.Where((x) => x.Drawing == drawing.Id).ToList();
            Random random = new Random();
            int winningTicket = random.Next(1, tickets.Count);
            Ticket ticket = tickets[winningTicket];
            Account account = db.Accounts.Where((x) => x.DiscordId == ticket.User).FirstOrDefault();
            account.Beer = account.Beer + drawing.Pool ?? 0;
            account.LifetimeBeer = account.LifetimeBeer + drawing.Pool ?? 0;
            db.Update(account);
            await db.SaveChangesAsync();

            drawing.WinningTicket = ticket.Number;
            drawing.WinningUserId = account.DiscordId;
            db.Update(drawing);
            await db.SaveChangesAsync();
            return account;
        }

        public async Task CreateTicket(Account account, int totalTickets)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Drawing drawing = await GetCurrentLottery();
            for (int i = 0; i < totalTickets; i++)
            {
                Ticket ticket = new Ticket()
                {
                    Drawing = drawing.Id,
                    Number = GenerateTicketNumber(),
                    User = account.DiscordId,
                    Timestamp = DateTime.UtcNow,
                };
                db.Add(ticket);
            }
            await db.SaveChangesAsync();
        }

        public async Task<int> GetTotalTickets()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Drawing drawing = await GetCurrentLottery();
            int totalTickets = db.Ticket.Where((x) => x.Drawing == drawing.Id).Count();
            return totalTickets;
        }

        public async Task<int> GetTotalLotteryParticipant()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Drawing drawing = await GetCurrentLottery();
            List<Ticket> tickets = db.Ticket.Where((x) => x.Drawing == drawing.Id).ToList();
            List<ulong> users = tickets.Select((x) => x.User).Distinct().ToList();
            return users.Count;
        }

        public async Task<int> GetCurrentDrawingId()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Drawing drawing = await GetCurrentLottery();
            return drawing.Id;
        }

        public async Task<int> GetCurrentPrizePool()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Drawing drawing = await GetCurrentLottery();
            return drawing.Pool ?? 0;
        }

        public async Task AddToPool(int amount)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Drawing drawing = await GetCurrentLottery();
            drawing.Pool += amount;
            db.Update(drawing);
            await db.SaveChangesAsync();
        }

        public async Task<List<Ticket>> GetUserTickets(Account account)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Drawing drawing = await GetCurrentLottery();
            return db.Ticket.Where((x) => x.User == account.DiscordId && x.Drawing == drawing.Id).ToList();
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

                ticket = salt.Next(20, 80);
                ticket = ticket + month;
                ticket = ticket - day;
                ticket = ticket - min;
                ticket = ticket - sec;
                ticket = ticket + salt.Next(1, 10);
            }
            return ticket;
        }
    }
}