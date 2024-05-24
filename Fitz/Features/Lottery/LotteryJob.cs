using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Core.Discord;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Features.Lottery.Models;
using Fitz.Variables;
using Fitz.Variables.Channels;
using Fitz.Variables.Emojis;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fitz.Features.Lottery
{
    public class LotteryJob : ITimedJob
    {
        private readonly DiscordClient dClient;
        private readonly LotteryService lotteryService;
        private readonly BankService bankService;
        private readonly AccountService accountService;
        private readonly BotLog botLog;

        public ulong Emoji => LotteryEmojis.Lottery;

        public int Interval => 1;

        private int Pool = 36;

        private int DaysToRunLottery = 1;

        public LotteryJob(DiscordClient dClient, LotteryService lotteryService, BankService bankService, AccountService accountService, BotLog botLog)
        {
            this.dClient = dClient;
            this.lotteryService = lotteryService;
            this.bankService = bankService;
            this.accountService = accountService;
            this.botLog = botLog;
        }

        public async Task Execute()
        {
            try
            {
                this.botLog.Information(LogConsoleSettings.Jobs, LotteryEmojis.Lottery, $"Starting Lottery Job...");
                // Get Current Lottery
                Models.Lottery currentDrawing = this.lotteryService.GetCurrentDrawing();

                if (currentDrawing == null)
                {
                    // Start new lottery
                    await this.lotteryService.StartNewLotteryAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(DaysToRunLottery), Pool);
                    currentDrawing = this.lotteryService.GetCurrentDrawing();
                    await this.HandleLotterySubscriptions();
                }

                // If lottery is over
                if (currentDrawing.EndDate < DateTime.UtcNow)
                {
                    // Determine lottery winner(s)
                    List<Winners> winners = await this.lotteryService.DecideWinners(currentDrawing);
                    if (winners.Count == 0 || winners == null)
                    {
                        // End lottery
                        await this.lotteryService.EndLotteryAsync(currentDrawing);

                        // Start new lottery and Roll over the prize pool into the next lottery
                        await this.lotteryService.StartNewLotteryAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(DaysToRunLottery), (currentDrawing.Pool.Value + Pool));

                        // Check to see if there are any lottery subscribers
                    }
                    else
                    {
                        // End lottery
                        await this.lotteryService.EndLotteryAsync(currentDrawing);

                        foreach (Winners winner in winners)
                        {
                            // DM the winner
                            await this.MessageWinner(winner.AccountId, currentDrawing);
                        }

                        // Start new lottery with new prize pool.
                        await this.lotteryService.StartNewLotteryAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(DaysToRunLottery), Pool);
                        await this.HandleLotterySubscriptions();
                    }
                    currentDrawing = this.lotteryService.GetCurrentDrawing();
                }

                DiscordChannel lotteryChannel = await this.dClient.GetChannelAsync(Waterbear.LotteryInfo);
                DiscordEmbedBuilder lotteryEmbed = new DiscordEmbedBuilder
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
                    $"{DiscordEmoji.FromName(this.dClient, ":ticket:")}Total Tickets: `{await lotteryService.GetTotalTickets()}`\n" +
                    $"{DiscordEmoji.FromGuildEmote(this.dClient, LotteryEmojis.User)}Total Users: `{await lotteryService.GetTotalLotteryParticipant()}`\n" +
                    $"{DiscordEmoji.FromName(this.dClient, ":clock2:")}Time Left: `{await this.lotteryService.GetRemainingHoursUntilNextDrawing()}` Hrs"
                };

                string winnerNames = string.Empty;
                List<Account> priorWinners = this.lotteryService.GetLastLotteryWinnerAccounts();
                if (priorWinners.Count == 0)
                {
                    winnerNames = "No prior winners.";
                }
                else
                {
                    foreach (Account winner in priorWinners)
                    {
                        winnerNames += $"{winner.Username}\n";
                    }
                }

                lotteryEmbed.AddField($"**Prior Lottery Winners**", $"{winnerNames}", true);
                try
                {
                    IAsyncEnumerable<DiscordMessage> lotteryMessages = lotteryChannel.GetMessagesAsync();
                    await foreach (DiscordMessage message in lotteryMessages)
                    {
                        if (message.Author.Id == this.dClient.CurrentUser.Id)
                        {
                            await message.ModifyAsync(content: "Use `/lottery` to get started.", embed: lotteryEmbed.Build());
                            this.botLog.Information(LogConsoleSettings.Jobs, LotteryEmojis.Lottery, $"Finished Lottery Job");
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occured when running lottery job.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occured when running lottery job.");
            }
        }

        private async Task HandleLotterySubscriptions()
        {
            List<Account> lotterySubscribers = this.accountService.GetLotterySubscribers();
            foreach (Account subscriber in lotterySubscribers)
            {
                if (subscriber == null)
                {
                    return;
                }
                if (subscriber.subscribeToLottery && subscriber.SubscribeTickets != 0)
                {
                    if (subscriber == null)
                    {
                        return;
                    }
                    // If the user's beer is equal to or less than the safe balance, do nothing.
                    if (subscriber.Beer <= subscriber.safeBalance)
                    {
                        break;
                    }
                    // If the user's beer is greater than the safe balance, buy tickets
                    else if (subscriber.Beer > subscriber.safeBalance)
                    {
                        // Only buy tickets if the user has enough beer to buy a ticket.
                        if (subscriber.Beer >= (subscriber.SubscribeTickets * 1))
                        {
                            // Check to see if the user has already bought tickets. We only allow 36 tickets per user.
                            List<Ticket> userTickets = lotteryService.GetUserTickets(subscriber);
                            if (userTickets.Count == 36)
                            {
                                // TODO: DM the user that the lottery tried to buy tickets for them but they were at the limit.
                                return;
                            }
                            if (userTickets.Count + subscriber.SubscribeTickets > 36)
                            {
                                // If the user is trying to buy more tickets than the limit, only buy up to the limit.
                            }
                            if (userTickets.Count + subscriber.SubscribeTickets <= 36)
                            {
                                // Buy the tickets for the user.

                                await bankService.PurchaseLotteryTicket(subscriber, subscriber.SubscribeTickets);

                                await lotteryService.CreateTicket(subscriber, subscriber.SubscribeTickets);

                                await lotteryService.AddToPool(subscriber.SubscribeTickets);

                                // TODO: DM the user that they have bought tickets.
                            }
                        }
                    }
                }
            }
        }

        private async Task MessageWinner(ulong userId, Models.Lottery drawing)
        {
            if (userId == 0)
            {
                return;
            }
            else
            {
                DiscordGuild guild = await this.dClient.GetGuildAsync(Guilds.Waterbear);
                DiscordMember member = await guild.GetMemberAsync(userId);
                if (member == null || member.IsBot)
                {
                    return;
                }
                // DM The winner to let them know.
                DiscordDmChannel userDMChannel = await member.CreateDmChannelAsync();

                DiscordEmbedBuilder lotteryEmbed = new DiscordEmbedBuilder
                {
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        IconUrl = DiscordEmoji.FromGuildEmote(this.dClient, LotteryEmojis.Ticket).Url,
                        Text = $"Lottery | #{drawing.Id}",
                    },
                    Color = new DiscordColor(52, 114, 53),
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    {
                        Url = DiscordEmoji.FromGuildEmote(this.dClient, LotteryEmojis.Lottery).Url,
                    },
                    Title = $"Congratulations! You've won lottery#{drawing.Id}!",
                    Timestamp = DateTime.UtcNow,
                    Description = $"Winnings have been distributed.\n" +
                    $"New beer balance: `{this.bankService.GetBalance(userId)}`",
                };
                await userDMChannel.SendMessageAsync(embed: lotteryEmbed.Build());
            }
        }
    }
}