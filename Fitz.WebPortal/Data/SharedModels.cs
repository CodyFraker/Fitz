using Fitz.Shared.Data;
using Fitz.Shared.Models;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Fitz.WebPortal.Data
{
    // Type aliases to help with the transition to shared models
    public class BotContext : Fitz.Shared.Data.BotContext
    {
        public BotContext(Microsoft.EntityFrameworkCore.DbContextOptions<Fitz.Shared.Data.BotContext> options) 
            : base(options)
        {
        }
    }

    // Type aliases for the models
    public class Account : Fitz.Shared.Models.Account { }
    public class Transaction : Fitz.Shared.Models.Transaction { }
    public class Lottery : Fitz.Shared.Models.Lottery { }
    public class LotteryEntry : Fitz.Shared.Models.LotteryEntry { }

    // Extension methods for model conversion
    public static class ModelExtensions
    {
        // Convert individual entities
        public static Account ToWebPortalAccount(this Fitz.Shared.Models.Account account)
        {
            if (account == null) return null;
            return new Account
            {
                Id = account.Id,
                Username = account.Username,
                Beer = account.Beer,
                LifetimeBeer = account.LifetimeBeer,
                SafeBalance = account.SafeBalance,
                Favorability = account.Favorability,
                SubscribeToLottery = account.SubscribeToLottery,
                SubscribeTickets = account.SubscribeTickets,
                Deactivated = account.Deactivated
            };
        }

        public static Transaction ToWebPortalTransaction(this Fitz.Shared.Models.Transaction transaction)
        {
            if (transaction == null) return null;
            return new Transaction
            {
                Id = transaction.Id,
                SenderId = transaction.SenderId,
                RecipientId = transaction.RecipientId,
                Amount = transaction.Amount,
                Timestamp = transaction.Timestamp,
                Reason = transaction.Reason
            };
        }

        public static Lottery ToWebPortalLottery(this Fitz.Shared.Models.Lottery lottery)
        {
            if (lottery == null) return null;
            var result = new Lottery
            {
                Id = lottery.Id,
                PrizePool = lottery.PrizePool,
                StartDate = lottery.StartDate,
                DrawDate = lottery.DrawDate,
                IsActive = lottery.IsActive,
                WinnerId = lottery.WinnerId
            };
            
            // Initialize with a new collection to avoid null reference
            result.Entries = new List<Fitz.Shared.Models.LotteryEntry>();
            
            // Copy entries if they exist
            if (lottery.Entries != null && lottery.Entries.Any())
            {
                foreach (var entry in lottery.Entries)
                {
                    result.Entries.Add(entry);
                }
            }
            
            return result;
        }

        public static LotteryEntry ToWebPortalLotteryEntry(this Fitz.Shared.Models.LotteryEntry entry)
        {
            if (entry == null) return null;
            return new LotteryEntry
            {
                Id = entry.Id,
                LotteryId = entry.LotteryId,
                AccountId = entry.AccountId,
                EntryDate = entry.EntryDate
            };
        }

        // Convert lists
        public static List<Account> ToWebPortalAccounts(this IEnumerable<Fitz.Shared.Models.Account> accounts)
        {
            if (accounts == null) return null;
            var result = new List<Account>();
            foreach (var account in accounts)
            {
                result.Add(account.ToWebPortalAccount());
            }
            return result;
        }

        public static List<Transaction> ToWebPortalTransactions(this IEnumerable<Fitz.Shared.Models.Transaction> transactions)
        {
            if (transactions == null) return null;
            var result = new List<Transaction>();
            foreach (var transaction in transactions)
            {
                result.Add(transaction.ToWebPortalTransaction());
            }
            return result;
        }

        public static List<Lottery> ToWebPortalLotteries(this IEnumerable<Fitz.Shared.Models.Lottery> lotteries)
        {
            if (lotteries == null) return null;
            var result = new List<Lottery>();
            foreach (var lottery in lotteries)
            {
                result.Add(lottery.ToWebPortalLottery());
            }
            return result;
        }

        public static List<LotteryEntry> ToWebPortalLotteryEntries(this IEnumerable<Fitz.Shared.Models.LotteryEntry> entries)
        {
            if (entries == null) return null;
            var result = new List<LotteryEntry>();
            foreach (var entry in entries)
            {
                result.Add(entry.ToWebPortalLotteryEntry());
            }
            return result;
        }

        // Conversion to shared models
        public static Fitz.Shared.Models.Account ToSharedAccount(this WebPortal.Data.Account account)
        {
            if (account == null) return null;
            return new Fitz.Shared.Models.Account
            {
                Id = account.Id,
                Username = account.Username,
                Beer = account.Beer,
                LifetimeBeer = account.LifetimeBeer,
                SafeBalance = account.SafeBalance,
                Favorability = account.Favorability,
                SubscribeToLottery = account.SubscribeToLottery,
                SubscribeTickets = account.SubscribeTickets,
                Deactivated = account.Deactivated
            };
        }
    }
} 