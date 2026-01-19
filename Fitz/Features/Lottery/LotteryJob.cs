using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Core.Discord;
using Fitz.Core.Services.Jobs;
using Fitz.Database.Entities;
using Fitz.Features.Accounts;
using Fitz.Features.Bank;
using Fitz.Features.Settings;
using Fitz.Metrics;
using Fitz.Variables;
using Fitz.Variables.Channels;
using Fitz.Variables.Emojis;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Lottery
{
    public class LotteryJob(DiscordClient dClient,
        LotteryService lotteryService,
        BankService bankService,
        AccountService accountService,
        BotLog botLog,
        SettingsService settingsService,
        FitzMetrics? fitzMetrics = null) : ITimedJob
    {
        private readonly DiscordClient dClient = dClient;
        private readonly LotteryService lotteryService = lotteryService;
        private readonly BankService bankService = bankService;
        private readonly AccountService accountService = accountService;
        private readonly BotLog botLog = botLog;
        private readonly SettingsService settingsService = settingsService;
        private readonly FitzMetrics? fitzMetrics = fitzMetrics;

        public ulong Emoji => LotteryEmojis.Lottery;

        public string Interval => CronInterval.Every1Minute;

        public async Task Execute()
        {
            var stopwatch = Stopwatch.StartNew();
            var jobName = "LotteryJob";
            
            try
            {
                this.botLog.Information(LogConsoleSettings.Jobs, LotteryEmojis.Lottery, $"Starting Lottery Job...");
                
                var subscribers = this.accountService.GetLotterySubscribers();
                fitzMetrics?.SetLotterySubscriptionsActive(subscribers.Count);

                // Get Current Lottery
                Database.Entities.LotteryEntity currentDrawing = this.lotteryService.GetCurrentLottery();

                // Get current settings
                Database.Entities.Settings settings = this.settingsService.GetSettings();

                if (currentDrawing == null)
                {
                    // Start new lottery
                    await this.lotteryService.StartNewLotteryAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(settings.LotteryDuration), settings.BaseLotteryPool);
                    currentDrawing = this.lotteryService.GetCurrentLottery();
                    await this.HandleLotterySubscriptions();
                }

                // If lottery is over
                if (currentDrawing.EndDate < DateTime.UtcNow)
                {
                    // Determine lottery winner(s)
                    List<WinnersEntity> winners = await this.lotteryService.DecideWinners(currentDrawing);
                    if (winners.Count == 0 || winners == null)
                    {
                        // End lottery
                        await this.lotteryService.EndLotteryAsync(currentDrawing);

                        // Start new lottery and Roll over the prize pool into the next lottery
                        await this.lotteryService.StartNewLotteryAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(settings.LotteryDuration), (currentDrawing.Pool.Value + settings.BaseLotteryPool));

                        // Check to see if there are any lottery subscribers
                        await this.HandleLotterySubscriptions();
                    }
                    else
                    {
                        // End lottery
                        await this.lotteryService.EndLotteryAsync(currentDrawing);

                        foreach (WinnersEntity winner in winners)
                        {
                            // DM the winner
                            await this.MessageWinner(winner.AccountId, currentDrawing);
                        }

                        // Start new lottery with new prize pool.
                        await this.lotteryService.StartNewLotteryAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(settings.LotteryDuration), settings.BaseLotteryPool);
                        await this.HandleLotterySubscriptions();
                    }
                    currentDrawing = this.lotteryService.GetCurrentLottery();
                }

                DiscordChannel lotteryChannel = await this.dClient.GetChannelAsync(Waterbear.LotteryInfo);
                DiscordEmbedBuilder lotteryEmbed = new()
                {
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        IconUrl = DiscordEmoji.FromGuildEmote(this.dClient, LotteryEmojis.Ticket).Url,
                        Text = $"Lottery#{currentDrawing.Id} | Last Winning Ticket: {this.lotteryService.GetLastWinningTicket()}",
                    },
                    Color = new DiscordColor(52, 114, 53),
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    {
                        Url = DiscordEmoji.FromGuildEmote(this.dClient, LotteryEmojis.Lottery).Url,
                    },
                    Title = $"Current Lottery Information",
                    Description = $"{DiscordEmoji.FromName(this.dClient, ":beer:")}Beer Pool: `{currentDrawing.Pool}` \n" +
                    $"{DiscordEmoji.FromGuildEmote(this.dClient, LotteryEmojis.Ticket)}Total Tickets: `{(int)lotteryService.GetTotalTickets().Data}`\n" +
                    $"{DiscordEmoji.FromGuildEmote(this.dClient, AccountEmojis.Users)}Total Users: `{(int)lotteryService.GetTotalLotteryParticipant().Data}`\n" +
                    $"{DiscordEmoji.FromName(this.dClient, ":clock2:")}Time Left: `{(int)this.lotteryService.GetRemainingHoursUntilNextDrawing().Data}` Hrs\n" +
                    $"Ticket cost: `{settings.TicketCost}` beer\n" +
                    $"Max Tickets per user: `{settings.MaxTickets}`"
                };

                string winnerNames = string.Empty;
                List<AccountEntity> priorWinners = this.lotteryService.GetLastLotteryWinnerAccounts();
                if (priorWinners.Count == 0)
                {
                    winnerNames = "No prior winners.";
                }
                else
                {
                    foreach (AccountEntity winner in priorWinners)
                    {
                        winnerNames += $"{winner.Username}\n";
                    }
                }

                lotteryEmbed.AddField($"**Prior Lottery Winners**", $"{winnerNames}", true);
                try
                {
                    IAsyncEnumerable<DiscordMessage> lotteryMessages = lotteryChannel.GetMessagesAsync();
                    if (lotteryMessages.ToBlockingEnumerable().Count() == 0)
                    {
                        await lotteryChannel.SendMessageAsync(embed: lotteryEmbed.Build());
                    }
                    await foreach (DiscordMessage message in lotteryMessages)
                    {
                        if (message.Author.Id == this.dClient.CurrentUser.Id)
                        {
                            await message.ModifyAsync(content: "Use `/lottery 0` for help.", embed: lotteryEmbed.Build());
                            this.botLog.Information(LogConsoleSettings.Jobs, LotteryEmojis.Lottery, $"Finished Lottery Job");
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occured when running lottery job.");
                }
                
                stopwatch.Stop();
                fitzMetrics?.RecordJobExecution(jobName, "success", stopwatch.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                fitzMetrics?.RecordJobExecution(jobName, "error", stopwatch.Elapsed.TotalSeconds);
                fitzMetrics?.RecordJobExecutionError(jobName);
                Log.Error(ex, "An error occured when running lottery job.");
            }
        }

        private async Task HandleLotterySubscriptions()
        {
            var settings = this.settingsService.GetSettings();
            List<AccountEntity> lotterySubscribers = this.accountService.GetLotterySubscribers();
            foreach (AccountEntity subscriber in lotterySubscribers)
            {
                if (subscriber == null)
                {
                    continue;
                }
                if (subscriber.subscribeToLottery && subscriber.SubscribeTickets != 0)
                {
                    // If the user's beer is equal to or less than the safe balance, do nothing.
                    if (subscriber.Beer <= subscriber.safeBalance)
                    {
                        continue;
                    }
                    // If the user's beer is greater than the safe balance, buy tickets
                    else if (subscriber.Beer > subscriber.safeBalance)
                    {
                        // Only buy tickets if the user has enough beer to buy a ticket.
                        if (subscriber.Beer >= (subscriber.SubscribeTickets * settings.TicketCost))
                        {
                            // Check to see if the user has already bought tickets.
                            List<TicketEntity> userTickets = lotteryService.GetUserTickets(subscriber).Data as List<TicketEntity>;
                            if (userTickets.Count == settings.MaxTickets)
                            {
                                // TODO: DM the user that the lottery tried to buy tickets for them but they were at the limit.
                                continue;
                            }
                            if (userTickets.Count + subscriber.SubscribeTickets > settings.MaxTickets)
                            {
                                // If the user is trying to buy more tickets than the limit, only buy up to the limit.
                            }
                            if (userTickets.Count + subscriber.SubscribeTickets <= settings.MaxTickets)
                            {
                                // Buy the tickets for the user.

                                await bankService.PurchaseLotteryTicket(subscriber, subscriber.SubscribeTickets);

                                await lotteryService.CreateTicket(subscriber, subscriber.SubscribeTickets);

                                await lotteryService.AddToPool(subscriber.SubscribeTickets);
                                userTickets = lotteryService.GetUserTickets(subscriber).Data as List<TicketEntity>;
                                await MessageEnrolleeSuccess(subscriber, userTickets);
                            }
                        }
                    }
                }
            }
        }

        private async Task MessageEnrolleeSuccess(AccountEntity account, List<TicketEntity> userTickets)
        {
            try
            {
                // Get discord user
                DiscordUser user = await this.dClient.GetUserAsync(account.Id);

                // Get current lottery
                Database.Entities.LotteryEntity drawing = this.lotteryService.GetCurrentLottery();

                DiscordEmbedBuilder lotteryEmbed = new DiscordEmbedBuilder
                {
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        IconUrl = DiscordEmoji.FromGuildEmote(this.dClient, LotteryEmojis.Ticket).Url,
                        Text = $"Lottery #{drawing.Id}",
                    },
                    Color = new DiscordColor(52, 114, 53),
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    {
                        Url = DiscordEmoji.FromGuildEmote(this.dClient, LotteryEmojis.Lottery).Url,
                    },
                    Title = $"Lottery Subscription",
                    Timestamp = DateTime.UtcNow,
                    Description = $"Since you've enrolled in the lottery, I went ahead and purchased {userTickets.Count} ticket(s) for you.\n\n" +
                    $"You can disable your lottery subscription via `/settings`.\n\n" +
                    $"Your current beer balance is: {account.Beer}.\n\n" +
                    $"I will not purchase any tickets if your beer is below {account.safeBalance}.\n\n" +
                    $"You can change your safe balance at any time via `/settings`",
                };
                DiscordGuild guild = await this.dClient.GetGuildAsync(Guilds.Waterbear);
                DiscordMember member = await guild.GetMemberAsync(user.Id);
                DiscordDmChannel userDMChannel = await member.CreateDmChannelAsync();
                await Task.Delay(5000);
                await userDMChannel.SendMessageAsync(embed: lotteryEmbed.Build());
            }
            catch (Exception ex)
            {
            }
        }

        private async Task MessageWinner(ulong userId, Database.Entities.LotteryEntity drawing)
        {
            if (userId == 0)
            {
                return;
            }
            else
            {
                List<AccountEntity> winners = this.lotteryService.GetLastLotteryWinnerAccounts();
                DiscordGuild guild = await this.dClient.GetGuildAsync(Guilds.Waterbear);
                DiscordMember member = await guild.GetMemberAsync(userId);
                if (member == null || member.IsBot)
                {
                    return;
                }

                // DM The winner to let them know.
                DiscordDmChannel userDMChannel = await member.CreateDmChannelAsync();

                await userDMChannel.SendMessageAsync(embed: this.lotteryService.WinnerEmbed(this.dClient, drawing, winners, userId));
            }
        }
    }
}