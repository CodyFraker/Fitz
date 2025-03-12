using Fitz.Features.Accounts.Models;
using Fitz.Features.Lottery.Models;
using Fitz.Features.Bank.Models;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Fitz.Core.Contexts;

namespace Fitz.WebPortal.Data
{
    // Type aliases to help with the transition to shared models
    public class BotContext : Fitz.Core.Contexts.BotContext
    {
        public BotContext(Microsoft.EntityFrameworkCore.DbContextOptions<Fitz.Core.Contexts.BotContext> options) 
            : base(options)
        {
        }

        public new DbSet<Account> Accounts { get; set; }
        public DbSet<Lottery> Lotteries { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
    }

    // Type aliases for the models
    public class Account : Fitz.Features.Accounts.Models.Account { }
    public class Transaction : Fitz.Features.Bank.Models.Transaction { }
    public class Lottery : Fitz.Features.Lottery.Models.Lottery { }
    public class Ticket : Fitz.Features.Lottery.Models.Ticket { }

    // Extension methods for model conversion
    public static class ModelExtensions
    {
        // Convert individual entities
        public static Account ToWebPortalAccount(this Fitz.Features.Accounts.Models.Account account)
        {
            if (account == null) return null;
            return new Account
            {
                Id = account.Id,
                Username = account.Username,
                Beer = account.Beer,
                LifetimeBeer = account.LifetimeBeer,
                safeBalance = account.safeBalance,
                Favorability = account.Favorability,
                CreatedDate = account.CreatedDate,
                LastSeenDate = account.LastSeenDate,
                LastActivityDate = account.LastActivityDate,
                subscribeToLottery = account.subscribeToLottery,
                SubscribeTickets = account.SubscribeTickets,
                Deactivated = account.Deactivated
            };
        }

        public static Lottery ToWebPortalLottery(this Fitz.Features.Lottery.Models.Lottery lottery)
        {
            if (lottery == null) return null;
            return new Lottery
            {
                Id = lottery.Id,
                StartDate = lottery.StartDate,
                EndDate = lottery.EndDate,
                Pool = lottery.Pool,
                CurrentLottery = lottery.CurrentLottery,
                WinningTicketId = lottery.WinningTicketId,
                WinningTicket = lottery.WinningTicket,
                Tickets = lottery.Tickets
            };
        }

        public static Ticket ToWebPortalTicket(this Fitz.Features.Lottery.Models.Ticket ticket)
        {
            if (ticket == null) return null;
            return new Ticket
            {
                Id = ticket.Id,
                LotteryId = ticket.LotteryId,
                Lottery = ticket.Lottery,
                UserId = ticket.UserId,
                PurchaseDate = ticket.PurchaseDate,
                TicketNumber = ticket.TicketNumber,
                IsWinner = ticket.IsWinner
            };
        }

        // Convert collections
        public static IEnumerable<Account> ToWebPortalAccounts(this IEnumerable<Fitz.Features.Accounts.Models.Account> accounts)
        {
            return accounts?.Select(a => a.ToWebPortalAccount());
        }

        public static IEnumerable<Lottery> ToWebPortalLotteries(this IEnumerable<Fitz.Features.Lottery.Models.Lottery> lotteries)
        {
            return lotteries?.Select(l => l.ToWebPortalLottery());
        }

        public static IEnumerable<Ticket> ToWebPortalTickets(this IEnumerable<Fitz.Features.Lottery.Models.Ticket> tickets)
        {
            return tickets?.Select(t => t.ToWebPortalTicket());
        }
    }
} 