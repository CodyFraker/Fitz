using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Core.Services.Settings;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Features.Lottery.Models;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Fitz.Features.Lottery
{
    public sealed class LotteryService(IServiceScopeFactory scopeFactory, AccountService accountService, BankService bankService, SettingsService settingsService, BotLog botLog)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly AccountService accountService = accountService;
        private readonly BankService bankService = bankService;
        private readonly SettingsService settingsService = settingsService;
        private readonly BotLog botLog = botLog;

        #region Get Lottery Details

        #region Get Current Lottery

        /// <summary>
        /// Get the most current lottery.
        /// </summary>
        /// <returns>Lottery</returns>
        public Models.Lottery GetCurrentLottery()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return db.Drawing.Where((x) => x.CurrentLottery == true).FirstOrDefault();
        }

        #endregion Get Current Lottery

        #region Get Remaining Time Until Next Drawing

        /// <summary>
        /// Return the remaining hours for the lottery to end.
        /// </summary>
        /// <returns></returns>
        public Result GetRemainingHoursUntilNextDrawing()
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                Models.Lottery lottery = this.GetCurrentLottery();
                int hours = (int)(lottery.EndDate - DateTime.UtcNow).TotalHours;
                return new Result(true, $"Got {hours} hour(s) back from current lottery.", hours);
            }
            catch (Exception ex)
            {
                return new Result(false, "Failed to get remaining hours until next drawing.", ex);
            }
        }

        #endregion Get Remaining Time Until Next Drawing

        #region Get Total Tickets

        /// <summary>
        /// Return the total tickets or entries in the current lottery.
        /// </summary>
        /// <returns></returns>
        public Result GetTotalTickets()
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                Models.Lottery drawing = this.GetCurrentLottery();
                int totalTickets = db.Ticket.Where((x) => x.Drawing == drawing.Id).Count();
                return new Result(true, $"Database returned {totalTickets} back", totalTickets);
            }
            catch (Exception ex)
            {
                return new Result(false, "Failed to get total tickets.", ex);
            }
        }

        public Result GetTotalTicketsForLottery(Models.Lottery lottery)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                int totalTickets = db.Ticket.Where((x) => x.Drawing == lottery.Id).Count();
                return new Result(true, $"Database returned {totalTickets} back", totalTickets);
            }
            catch (Exception ex)
            {
                return new Result(false, "Failed to get total tickets.", ex);
            }
        }

        #endregion Get Total Tickets

        #region Get Total Lottery Participants

        /// <summary>
        /// Return the total amount of participants in the current lottery.
        /// </summary>
        /// <returns></returns>
        public Result GetTotalLotteryParticipant()
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                Models.Lottery drawing = this.GetCurrentLottery();
                List<Ticket> tickets = [.. db.Ticket.Where((x) => x.Drawing == drawing.Id)];
                List<ulong> users = tickets.Select((x) => x.AccountId).Distinct().ToList();
                return new Result(true, $"Database returned {users.Count} back", users.Count);
            }
            catch (Exception ex)
            {
                return new Result(false, "Failed to get total lottery participants.", ex);
            }
        }

        public Result GetTotalLotteryParticipantsByLottery(Models.Lottery lottery)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                List<Ticket> tickets = [.. db.Ticket.Where((x) => x.Drawing == lottery.Id)];
                List<ulong> users = tickets.Select((x) => x.AccountId).Distinct().ToList();
                return new Result(true, $"Database returned {users.Count} back", users.Count);
            }
            catch (Exception ex)
            {
                return new Result(false, "Failed to get total lottery participants.", ex);
            }
        }

        #endregion Get Total Lottery Participants

        #region Get User Tickets

        /// <summary>
        /// Returns the tickets for a specific user.
        /// </summary>
        /// <param name="account">User's Account</param>
        /// <returns>List of tickets purchased by a user. If zero, it'll return null.</returns>
        public Result GetUserTickets(Account account)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                Models.Lottery drawing = this.GetCurrentLottery();

                List<Ticket> userTickets = [.. db.Ticket.Where((x) => x.AccountId == account.Id && x.Drawing == drawing.Id)];
                if (userTickets == null || userTickets.Count == 0)
                {
                    return new Result(true, "User has no tickets.", new List<Ticket>());
                }
                return new Result(true, $"User has {userTickets.Count} tickets.", userTickets);
            }
            catch (Exception ex)
            {
                return new Result(false, "Failed to get user tickets.", ex);
            }
        }

        /// <summary>
        /// Get all tickets for a specific user.
        /// </summary>
        /// <param name="userId">Account ID</param>
        /// <returns>List of tickets a user has purchased for all lotteries.</returns>
        public List<Ticket> GetTicketsByUserId(ulong userId)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            List<Ticket> tickets = db.Ticket.Where((x) => x.AccountId == userId).ToList();
            return tickets;
        }

        public int GetTotalLotteryPartipationsByUserId(ulong userId)
        {
            // get distinct amount of lottery entries by user
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            List<Ticket> tickets = db.Ticket.Where((x) => x.AccountId == userId).ToList();
            List<int> drawings = tickets.Select((x) => x.Drawing).Distinct().ToList();
            return drawings.Count;
        }

        public int GetLargestPayoutByUserId(ulong userId)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            List<Winners> winners = db.Winners.Where((x) => x.AccountId == userId).ToList();
            if (winners.Count == 0)
            {
                return 0;
            }
            return winners.Max((x) => x.Payout);
        }

        public int GetTotalWinsByAccountId(ulong accountId)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return db.Winners.Where((x) => x.AccountId == accountId).Count();
        }

        #endregion Get User Tickets

        #region Get Last Winning Ticket

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

        #endregion Get Last Winning Ticket

        #region Get Last Lottery Winner Accounts

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

            List<Winners> winners = [.. db.Winners.Where((x) => x.Drawing == drawing.Id)];
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

        #endregion Get Last Lottery Winner Accounts

        #endregion Get Lottery Details

        #region Lottery Management

        /// <summary>
        /// Starts a new lottery.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="pool"></param>
        /// <returns></returns>
        public async Task StartNewLotteryAsync(DateTime startDate, DateTime endDate, int pool = 0)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
                Settings settings = this.settingsService.GetSettings();

                if (startDate == null)
                {
                    startDate = DateTime.UtcNow;
                }
                // Default End date to 7 days from now.
                if (endDate == null)
                {
                    endDate = DateTime.UtcNow.AddDays(settings.LotteryDuration);
                }

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
                Log.Debug($"Started new lottery with ID: {drawing.Id} | End Date: {drawing.EndDate}");
                this.botLog.Information(LogConsoleSettings.LotteryLog, LotteryEmojis.Lottery, $"Started new lottery with ID: {drawing.Id} | End Date: {drawing.EndDate}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to start new lottery.");
                this.botLog.Error($"Failed to start new lottery. | Stack trace: {ex.StackTrace}");
            }
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
            this.botLog.Information(LogConsoleSettings.LotteryLog, LotteryEmojis.Lottery, $"Ended lottery with ID: {currentLottery.Id} | Winning Ticket: {currentLottery.WinningTicket}");
        }

        #endregion Lottery Management

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
                    this.botLog.Information(LogConsoleSettings.LotteryLog, LotteryEmojis.Lottery, $"User {account.Username} won {payout} beer in the lottery.");
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

            Models.Lottery drawing = this.GetCurrentLottery();
            drawing.EndDate = endDate;
            db.Update(drawing);
            await db.SaveChangesAsync();
            this.botLog.Information(LogConsoleSettings.LotteryLog, LotteryEmojis.Lottery, $"Updated lottery ID: {drawing.Id}");
        }

        public async Task<Result> SetLotteryPrizePoolAsync(int pool)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                Models.Lottery drawing = this.GetCurrentLottery();
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

        public async Task<Result> CreateTicket(Account account, int totalTickets)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                // Get User Account Settings
                Settings settings = this.settingsService.GetSettings();

                // Get account tickets for this current lottery.
                var accountTicketsResult = this.GetUserTickets(account);
                List<Ticket> accountTickets = new List<Ticket>();

                if (accountTicketsResult.Success == true)
                {
                    if (accountTickets == null)
                    {
                        accountTickets = new List<Ticket>();
                    }
                    else
                    {
                        accountTickets = this.GetUserTickets(account).Data as List<Ticket>;
                    }
                }
                else
                {
                    return accountTicketsResult;
                }

                Models.Lottery drawing = this.GetCurrentLottery();
                for (int i = 0; i < totalTickets; i++)
                {
                    if (accountTickets.Count >= settings.MaxTickets && account.Id != Users.Fitz)
                    {
                        // User already has max amount of tickets.No more can be added to this current lottery.
                        // If the user is Fitz, we're going to ignore this as Fitz can have as many tickets as he wants.
                        return new Result(false, "User already has max amount of tickets.", accountTickets);
                    }
                    int ticketNumber = GenerateTicketNumber();

                    // Check to see if we've already generated a unique ticket number for this user.
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
                        --i;
                        await GenerateTicketForFitz();
                    }
                }
                return new Result(true, $"Successfully bought {totalTickets} tickets for {account.Username}.", accountTickets);
            }
            catch (Exception ex)
            {
                return new Result(false, "Failed to create ticket.", ex);
            }
        }

        public async Task GenerateTicketForFitz()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            // Get account tickets for this current lottery.
            List<Ticket> accountTickets = this.GetUserTickets(accountService.FindAccount(Users.Fitz)).Data as List<Ticket>;

            Models.Lottery drawing = this.GetCurrentLottery();
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

        public async Task AddToPool(int amount)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Models.Lottery drawing = this.GetCurrentLottery();
            drawing.Pool += amount;
            db.Update(drawing);
            await db.SaveChangesAsync();
        }

        #region Embeds

        public DiscordEmbed WinnerEmbed(DiscordClient dClient, Models.Lottery lottery, List<Account> winners)
        {
            string multiWinners = string.Empty;
            if (winners.Count() > 1)
            {
                multiWinners = $"With a total of {winners.Count()} winner(s), you've won `{lottery.Pool / winners.Count()}`\n";
            }

            DiscordEmbedBuilder lotteryEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Ticket).Url,
                    Text = $"Lottery #{lottery.Id}",
                },
                Color = new DiscordColor(52, 114, 53),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Lottery).Url,
                },
                Title = $"Congratulations! You've won lottery #{lottery.Id}!",
                Timestamp = DateTime.UtcNow,
                Description = $"The total prize pool was `{lottery.Pool}`\n" +
                $"Total tickets: `{GetTotalTicketsForLottery(lottery).Data}`\n" +
                $"Total Users: `{GetTotalLotteryParticipantsByLottery(lottery).Data}`\n" +
                $"{multiWinners}" +
                $"New beer balance: `BEER AMOUNT`",
            };

            return lotteryEmbed;
        }

        #endregion Embeds

        private int GenerateTicketNumber()
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[4];
                int ticketNumber = 0;
                for (int i = 0; i < 4; i++)
                {
                    rng.GetBytes(data);
                    ticketNumber = BitConverter.ToInt32(data, 0);
                    ticketNumber = Math.Abs(ticketNumber);
                    ticketNumber %= 1000;
                }
                if (ticketNumber >= 0 && ticketNumber <= 1000)
                {
                    return ticketNumber;
                }
                else
                {
                    return GenerateTicketNumber();
                }
            }
        }
    }
}