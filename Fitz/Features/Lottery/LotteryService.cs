using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Database;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Database.Entities;
using Fitz.Features.Accounts;
using Fitz.Features.Bank;
using Fitz.Features.Settings;
using Fitz.Metrics;
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
    public sealed class LotteryService(IServiceScopeFactory scopeFactory, AccountService accountService, BankService bankService, SettingsService settingsService, BotLog botLog, FitzMetrics? fitzMetrics = null)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly AccountService accountService = accountService;
        private readonly BankService bankService = bankService;
        private readonly SettingsService settingsService = settingsService;
        private readonly BotLog botLog = botLog;
        private readonly FitzMetrics? fitzMetrics = fitzMetrics;

        #region Get Lottery Details

        #region Get Current Lottery

        /// <summary>
        /// Get the most current lottery.
        /// </summary>
        /// <returns>Lottery</returns>
        public Database.Entities.LotteryEntity GetCurrentLottery()
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

                Database.Entities.LotteryEntity lottery = this.GetCurrentLottery();
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

                Database.Entities.LotteryEntity drawing = this.GetCurrentLottery();
                int totalTickets = db.Ticket.Where((x) => x.Drawing == drawing.Id).Count();
                return new Result(true, $"Database returned {totalTickets} back", totalTickets);
            }
            catch (Exception ex)
            {
                return new Result(false, "Failed to get total tickets.", ex);
            }
        }

        public Result GetTotalTicketsForLottery(Database.Entities.LotteryEntity lottery)
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

                Database.Entities.LotteryEntity drawing = this.GetCurrentLottery();
                List<TicketEntity> tickets = [.. db.Ticket.Where((x) => x.Drawing == drawing.Id)];
                List<ulong> users = tickets.Select((x) => x.AccountId).Distinct().ToList();
                return new Result(true, $"Database returned {users.Count} back", users.Count);
            }
            catch (Exception ex)
            {
                return new Result(false, "Failed to get total lottery participants.", ex);
            }
        }

        public Result GetTotalLotteryParticipantsByLottery(Database.Entities.LotteryEntity lottery)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                List<TicketEntity> tickets = [.. db.Ticket.Where((x) => x.Drawing == lottery.Id)];
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
        public Result GetUserTickets(AccountEntity account)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                Database.Entities.LotteryEntity drawing = this.GetCurrentLottery();

                List<TicketEntity> userTickets = [.. db.Ticket.Where((x) => x.AccountId == account.Id && x.Drawing == drawing.Id)];
                if (userTickets == null || userTickets.Count == 0)
                {
                    return new Result(true, "User has no tickets.", new List<TicketEntity>());
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
        public List<TicketEntity> GetTicketsByUserId(ulong userId)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            List<TicketEntity> tickets = db.Ticket.Where((x) => x.AccountId == userId).ToList();
            return tickets;
        }

        public int GetTotalLotteryPartipationsByUserId(ulong userId)
        {
            // get distinct amount of lottery entries by user
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            List<TicketEntity> tickets = db.Ticket.Where((x) => x.AccountId == userId).ToList();
            List<int> drawings = tickets.Select((x) => x.Drawing).Distinct().ToList();
            return drawings.Count;
        }

        public int GetLargestPayoutByUserId(ulong userId)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            List<WinnersEntity> winners = db.Winners.Where((x) => x.AccountId == userId).ToList();
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

            Database.Entities.LotteryEntity drawing = db.Drawing.Where((x) => x.CurrentLottery == false).OrderByDescending((x) => x.EndDate).FirstOrDefault();

            if (drawing == null)
            {
                return 0;
            }
            return drawing.WinningTicket ?? 0;
        }

        #endregion Get Last Winning Ticket

        #region Get Last Lottery Winner Accounts

        public List<AccountEntity> GetLastLotteryWinnerAccounts()
        {
            List<AccountEntity> accounts = new List<AccountEntity>();
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            // Just use the method to get last drawing instead of calling it everywhere else.
            Database.Entities.LotteryEntity drawing = db.Drawing.Where((x) => x.CurrentLottery == false).OrderByDescending((x) => x.EndDate).FirstOrDefault();

            if (drawing == null)
            {
                return accounts;
            }

            List<WinnersEntity> winners = [.. db.Winners.Where((x) => x.Drawing == drawing.Id)];
            foreach (WinnersEntity winner in winners)
            {
                AccountEntity account = db.Accounts.Where((x) => x.Id == winner.AccountId).FirstOrDefault();
                if (accounts.Contains(account) == false)
                {
                    accounts.Add(account);
                }
            }
            return accounts;
        }

        #endregion Get Last Lottery Winner Accounts

        #region Get Lottery History

        /// <summary>
        /// Get paginated history of past lotteries.
        /// </summary>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <returns>Tuple containing list of lotteries and total count</returns>
        public (List<Database.Entities.LotteryEntity> lotteries, int totalCount) GetLotteryHistory(int skip, int take)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            var query = db.Drawing.Where(x => x.CurrentLottery == false)
                .OrderByDescending(x => x.EndDate);

            int totalCount = query.Count();
            var lotteries = query.Skip(skip).Take(take).ToList();

            return (lotteries, totalCount);
        }

        #endregion Get Lottery History

        #region Get Lottery Statistics

        /// <summary>
        /// Get time-series statistics for all lotteries (current and past).
        /// </summary>
        /// <returns>List of statistics points with date, prize pool, and total tickets</returns>
        public List<(DateTime date, int prizePool, int totalTickets)> GetLotteryStatistics()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            var allLotteries = db.Drawing.OrderBy(x => x.StartDate).ToList();
            var statistics = new List<(DateTime date, int prizePool, int totalTickets)>();

            foreach (var lottery in allLotteries)
            {
                var totalTicketsResult = GetTotalTicketsForLottery(lottery);
                int totalTickets = totalTicketsResult.Success ? (int)totalTicketsResult.Data : 0;
                int prizePool = lottery.Pool ?? 0;

                statistics.Add((lottery.StartDate, prizePool, totalTickets));
            }

            return statistics;
        }

        #endregion Get Lottery Statistics

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
                Database.Entities.Settings settings = this.settingsService.GetSettings();

                if (startDate == null)
                {
                    startDate = DateTime.UtcNow;
                }
                // Default End date to 7 days from now.
                if (endDate == null)
                {
                    endDate = DateTime.UtcNow.AddDays(settings.LotteryDuration);
                }

                Database.Entities.LotteryEntity drawing = new Database.Entities.LotteryEntity()
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    Pool = pool,
                    WinningTicket = null,
                    CurrentLottery = true,
                };
                db.Add(drawing);
                await db.SaveChangesAsync();
                
                var duration = (endDate - startDate).TotalSeconds;
                fitzMetrics?.RecordLotteryDuration(duration);
                fitzMetrics?.SetLotteryPoolSize(pool);
                
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
        public async Task EndLotteryAsync(Database.Entities.LotteryEntity currentLottery)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            currentLottery.CurrentLottery = false;
            db.Update(currentLottery);
            await db.SaveChangesAsync();
            this.botLog.Information(LogConsoleSettings.LotteryLog, LotteryEmojis.Lottery, $"Ended lottery with ID: {currentLottery.Id} | Winning Ticket: {currentLottery.WinningTicket}");
        }

        /// <summary>
        /// Ends the current lottery and decides winners.
        /// </summary>
        /// <param name="currentLottery">The current lottery to end</param>
        /// <returns></returns>
        public async Task EndLotteryAndDecideWinnersAsync(Database.Entities.LotteryEntity currentLottery)
        {
            List<WinnersEntity> winners = await this.DecideWinners(currentLottery);
            await this.EndLotteryAsync(currentLottery);
            
            if (winners.Count > 0)
            {
                this.botLog.Information(LogConsoleSettings.LotteryLog, LotteryEmojis.Lottery, $"Lottery {currentLottery.Id} ended with {winners.Count} winner(s)");
            }
            else
            {
                this.botLog.Information(LogConsoleSettings.LotteryLog, LotteryEmojis.Lottery, $"Lottery {currentLottery.Id} ended with no winners");
            }
        }

        #endregion Lottery Management

        /// <summary>
        /// Decide the winners of the current lottery.
        /// </summary>
        /// <param name="drawing"></param>
        /// <returns>List of Winners.</returns>
        public async Task<List<WinnersEntity>> DecideWinners(Database.Entities.LotteryEntity drawing)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            // Get all tickets from DB for this current lottery.
            List<TicketEntity> tickets = db.Ticket.Where((x) => x.Drawing == drawing.Id).ToList();
            List<WinnersEntity> winners = new List<WinnersEntity>();

            int winningTicket = 0;

            // For every ticket we have in the current lottery, generate a winning ticket number.
            // After X amount of tickets, we will have decided a winning ticket number.
            foreach (TicketEntity ticket in tickets)
            {
                winningTicket = GenerateTicketNumber();
            }
            // Save winning ticket information to the lottery.
            drawing.WinningTicket = winningTicket;
            db.Update(drawing);
            await db.SaveChangesAsync();

            // Check to see if any tickets in the current lottery match our winning ticket number.
            List<TicketEntity> winningTickets = tickets.Where(tickets => tickets.Number == winningTicket).ToList();

            if (winningTickets.Count > 0)
            {
                // Get all accounts who has a winning ticket
                List<AccountEntity> accounts = new List<AccountEntity>();
                foreach (TicketEntity ticket in winningTickets)
                {
                    AccountEntity account = db.Accounts.Where((x) => x.Id == ticket.AccountId).FirstOrDefault();
                    accounts.Add(account);
                }
                int payout = (drawing.Pool ?? 0) / accounts.Count;

                foreach (AccountEntity account in accounts)
                {
                    // stupid and bad code. I'm sorry.
                    WinnersEntity winner = new WinnersEntity()
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
                
                fitzMetrics?.RecordLotteryDrawing(winners.Count);
                
                return winners;
            }
            return winners;
        }

        public async Task UpdateCurrentLottery(DateTime endDate)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Database.Entities.LotteryEntity drawing = this.GetCurrentLottery();
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

                Database.Entities.LotteryEntity drawing = this.GetCurrentLottery();
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

        #region Buy Tickets for User

        public async Task<Result> BuyTicketsForUser(AccountEntity account, int tickets)
        {
            try
            {
                // Get the settings for the lottery.
                Database.Entities.Settings settings = settingsService.GetSettings();
                if (settings == null || settings.MaxTickets == 0)
                {
                    return new Result(false, "Failed to get lottery settings.", settings);
                }

                // Retrieve user tickets for the current lottery.
                List<TicketEntity> userTickets = new List<TicketEntity>();
                var getUserTicketsResult = this.GetUserTickets(account);
                if (!getUserTicketsResult.Success)
                {
                    return new Result(false, "Failed to get user tickets.", getUserTicketsResult.Message);
                }
                userTickets = getUserTicketsResult.Data as List<TicketEntity>;

                // Check to see if the user already has the max amount of tickets.
                if (userTickets.Count >= settings.MaxTickets)
                {
                    return new Result(false, "User already has max amount of tickets.", userTickets);
                }

                int totalBuyableTickets = settings.MaxTickets - userTickets.Count;
                if (tickets > totalBuyableTickets)
                {
                    if (totalBuyableTickets > 0)
                    {
                        tickets = totalBuyableTickets;
                    }
                }

                // Check to see if the user has enough beer to buy tickets.
                int totalTicketCost = tickets * settings.TicketCost;
                if (account.Beer < totalTicketCost)
                {
                    return new Result(false, "User does not have enough beer to buy tickets.", account);
                }

                // Create the ticket(s) for the account.
                var buyTicketsResult = await this.CreateTicket(account, tickets);
                if (!buyTicketsResult.Success)
                {
                    return new Result(false, "Failed to buy max tickets for user.", buyTicketsResult.Message);
                }

                // Deduct the cost of the tickets from the user's beer balance.
                var purchaseLotteryTicketResult = await this.bankService.PurchaseLotteryTicket(account, totalTicketCost);
                if (!purchaseLotteryTicketResult.Success)
                {
                    return new Result(false, "Failed to purchase lottery tickets for user.", purchaseLotteryTicketResult.Message);
                }

                var addToPoolResult = await this.AddToPool(totalTicketCost);
                if (!addToPoolResult.Success)
                {
                    return new Result(false, "Failed to add to pool.", addToPoolResult.Message);
                }

                fitzMetrics?.RecordLotteryTicketPurchase(tickets, totalTicketCost);

                userTickets = this.GetUserTickets(account).Data as List<TicketEntity>;

                return new Result(true, "Successfully bought max tickets for user.", userTickets);
            }
            catch (Exception ex)
            {
                return new Result(false, "Failed to buy max tickets for user.", ex);
            }
        }

        #endregion Buy Tickets for User

        public async Task<Result> CreateTicket(AccountEntity account, int totalTickets)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                // Get User Account Settings
                Fitz.Database.Entities.Settings settings = this.settingsService.GetSettings();

                // Get account tickets for this current lottery.
                var accountTicketsResult = this.GetUserTickets(account);

                List<TicketEntity> accountTickets = new List<TicketEntity>();

                if (accountTicketsResult.Success == true)
                {
                    if (accountTickets == null)
                    {
                        accountTickets = new List<TicketEntity>();
                    }
                    else
                    {
                        accountTickets = this.GetUserTickets(account).Data as List<TicketEntity>;
                    }
                }
                else
                {
                    return accountTicketsResult;
                }

                Database.Entities.LotteryEntity drawing = this.GetCurrentLottery();
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
                        TicketEntity newTicket = new TicketEntity()
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
            List<TicketEntity> accountTickets = this.GetUserTickets(accountService.FindAccount(Users.Fitz)).Data as List<TicketEntity>;

            Database.Entities.LotteryEntity drawing = this.GetCurrentLottery();
            int ticketNumber = GenerateTicketNumber();
            if (!accountTickets.Any((x) => x.Number == ticketNumber))
            {
                TicketEntity newTicket = new TicketEntity()
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
                AccountEntity fitz = accountService.FindAccount(Users.Fitz);
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

        public async Task<Result> AddToPool(int amount)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                Database.Entities.LotteryEntity drawing = this.GetCurrentLottery();
                drawing.Pool += amount;
                db.Update(drawing);
                await db.SaveChangesAsync();

                fitzMetrics?.SetLotteryPoolSize(drawing.Pool ?? 0);

                return new Result(true, $"Successfully added {amount} to the pool.", drawing);
            }
            catch (Exception ex)
            {
                return new Result(false, "Failed to add to pool.", ex);
            }
        }

        #region Embeds

        public DiscordEmbed WinnerEmbed(DiscordClient dClient, Database.Entities.LotteryEntity lottery, List<AccountEntity> winners, ulong userId)
        {
            string multiWinners = string.Empty;
            if (winners.Count() > 1)
            {
                multiWinners = $"With a total of {winners.Count()} winner(s), you've won `{lottery.Pool / winners.Count()}`\n";
            }

            AccountEntity winnerAccount = this.accountService.FindAccount(userId);

            DiscordEmbedBuilder lotteryEmbed = new()
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
                $"New beer balance: `{winnerAccount.Beer}`",
            };

            return lotteryEmbed;
        }

        public DiscordEmbed LotteryInfoEmbed(DiscordClient dClient, Database.Entities.LotteryEntity lottery, int daysLeft, List<TicketEntity> userTickets = null)
        {
            DiscordEmbedBuilder lotteryEmbed;
            if (userTickets == null)
            {
                lotteryEmbed = new DiscordEmbedBuilder
                {
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        IconUrl = DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Lottery).Url,
                        Text = $"Lottery #{lottery.Id} Time Left: {daysLeft} Hrs",
                    },
                    Color = new DiscordColor(52, 114, 53),
                    //Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    //{
                    //    Url = DiscordEmoji.FromGuildEmote(ctx.Client, LotteryEmojis.Lottery).Url,
                    //},
                    Title = $"Current Lottery Information",
                    Description = $"**Your Entries**: ```{userTickets.Count}```\n" +
                $"Your tickets are stored in the QR code.\n" +
                $"To see their values, run `/mytickets`"
                };
                //lotteryEmbed.AddField($"**{DiscordEmoji.FromName(this.dClient, ":beer:")}Fridge**", $"```{lottery.Pool}```", true);
                //lotteryEmbed.AddField($"**{DiscordEmoji.FromGuildEmote(this.dClient, LotteryEmojis.User)}Participants**", $"```{await lotteryService.GetTotalLotteryParticipant()}```", true);
                //lotteryEmbed.AddField($"**{DiscordEmoji.FromName(this.dClient, ":ticket:")}Entries**", $"```{await lotteryService.GetTotalTickets()}```", true);

                //lotteryEmbed.AddField($"**Starts**", $"```{lottery.StartDate}```", false);
                //lotteryEmbed.AddField($"**Ends**", $"```{lottery.EndDate}```", false);
            }
            else
            {
                lotteryEmbed = new DiscordEmbedBuilder
                {
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        IconUrl = DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Ticket).Url,
                        Text = $"Lottery #{lottery.Id} | Time Left: {daysLeft} Hrs",
                    },
                    Color = new DiscordColor(52, 114, 53),
                    //Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    //{
                    //    Url = DiscordEmoji.FromGuildEmote(ctx.Client, LotteryEmojis.Lottery).Url,
                    //},
                    Title = $"Current Lottery Information",
                    Description = $"**__Your Entries__**: ```ansi\n\u001b[1;37m{userTickets.Count}\u001b[0;0m\n```\n" +
                    $"Your tickets are stored in the QR code.\n" +
                    $"To see their values, run `/mytickets`"
                };
                lotteryEmbed.WithThumbnail(url: $"attachment://qrCode.png");
                lotteryEmbed.AddField($"**{DiscordEmoji.FromName(dClient, ":beer:")}Fridge**", $"```ansi\n\u001b[0;36m{lottery.Pool}\u001b[0;0m\n```", true);
                lotteryEmbed.AddField($"**{DiscordEmoji.FromGuildEmote(dClient, AccountEmojis.Users)}Participants**", $"```ansi\n\u001b[0;36m{(int)this.GetTotalLotteryParticipant().Data}\u001b[0;0m\n```", true);
                lotteryEmbed.AddField($"**{DiscordEmoji.FromName(dClient, ":ticket:")}Entries**", $"```ansi\n\u001b[0;36m{(int)this.GetTotalTickets().Data}\u001b[0;0m\n```", true);

                lotteryEmbed.AddField($"**Starts**", $"```ansi\n\u001b[1;33m{lottery.StartDate}\u001b[0;0m\n```", false);
                lotteryEmbed.AddField($"**Ends**", $"```ansi\n\u001b[1;31m{lottery.EndDate}\u001b[0;0m\n```", false);
            }

            return lotteryEmbed;
        }

        public DiscordEmbed LotteryCommandEmbed(DiscordClient dClient, Database.Entities.LotteryEntity lottery, Fitz.Database.Entities.Settings settings, AccountEntity account, List<TicketEntity> userTickets = null)
        {
            DiscordEmbedBuilder lotteryEmbed = new()
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Ticket).Url,
                    Text = $"Lottery #{lottery.Id} | Last Winning Ticket: {this.GetLastWinningTicket()}",
                },
                Color = new DiscordColor(52, 114, 53),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Lottery).Url,
                },
                Title = $"Lottery",
                Description =
                $"**__Your Entries__**: ```ansi\n\u001b[1;37m{userTickets.Count}\u001b[0;0m\n```\n" +
                $"**__Entries Available__**: ```{settings.MaxTickets - userTickets.Count}```\n"
            };

            lotteryEmbed.AddField($"Info",
                $"{DiscordEmoji.FromName(dClient, ":beer:")} Beer Pool: `{lottery.Pool}` \n" +
                $"{DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Ticket)} Total Tickets: `{(int)this.GetTotalTickets().Data}`\n" +
                $"{DiscordEmoji.FromGuildEmote(dClient, AccountEmojis.Users)} Total Users: `{(int)this.GetTotalLotteryParticipant().Data}`\n" +
                $"{DiscordEmoji.FromName(dClient, ":clock2:")} Time Left: `{(int)this.GetRemainingHoursUntilNextDrawing().Data}` Hrs\n" +
                $"Ticket cost: `{settings.TicketCost}` beer\n", false);

            lotteryEmbed.AddField($"**Starts**", $"```ansi\n\u001b[1;33m{lottery.StartDate}\u001b[0;0m\n```", false);
            lotteryEmbed.AddField($"**Ends**", $"```ansi\n\u001b[1;31m{lottery.EndDate}\u001b[0;0m\n```", false);
            return lotteryEmbed;
        }

        public DiscordEmbed LotteryEmbed(DiscordClient dClient, Database.Entities.LotteryEntity lottery, Database.Entities.Settings settings)
        {
            DiscordEmbedBuilder lotteryEmbed = new()
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Ticket).Url,
                    Text = $"Lottery#{lottery.Id} | Last Winning Ticket: {this.GetLastWinningTicket()}",
                },
                Color = new DiscordColor(52, 114, 53),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Lottery).Url,
                },
                Title = $"Current Lottery Information",
                Description = $"{DiscordEmoji.FromName(dClient, ":beer:")}Beer Pool: `{lottery.Pool}` \n" +
                $"{DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Ticket)}Total Tickets: `{(int)this.GetTotalTickets().Data}`\n" +
                $"{DiscordEmoji.FromGuildEmote(dClient, AccountEmojis.Users)}Total Users: `{(int)this.GetTotalLotteryParticipant().Data}`\n" +
                $"{DiscordEmoji.FromName(dClient, ":clock2:")}Time Left: `{(int)this.GetRemainingHoursUntilNextDrawing().Data}` Hrs\n" +
                $"Ticket cost: `{settings.TicketCost}` beer\n" +
                $"Max Tickets per user: `{settings.MaxTickets}`"
            };

            return lotteryEmbed;
        }

        public DiscordEmbed LotteryHelpEmbed(DiscordClient dClient, Database.Entities.LotteryEntity lottery, Fitz.Database.Entities.Settings settings)
        {
            DiscordEmbedBuilder lotteryHelpEmbed = new DiscordEmbedBuilder
            {
                Title = "Lottery Help",
                Description = "A single ticket will grant you a chance of 1-1001. You can purchase up to 36 tickets. None of them will be a duplicate ticket.\n" +
                    "If no one wins, the fridge will roll over into the next lottery, increasing the total beer.\n" +
                    "Favorability is factored when more than one person wins.\n" +
                    "I also play the lottery. I have no limit on the amount of tickets I can have.",
                Color = new DiscordColor(52, 114, 53),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Lottery).Url
                },
            };

            lotteryHelpEmbed.AddField($"Commands",
                $"`/lottery #` will buy a set amount of tickets. Providing 0 tickets will return this message again.\n" +
                $"\n" +
                $"`/lotteryinfo` will show you some basic information about the current drawing. The QR code will show you which tickets you have in this drawing.\n" +
                $"\n" +
                $"You can set your account to automatically play the lottery for by doing `/settings`.", false);

            return lotteryHelpEmbed;
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