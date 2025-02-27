using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Core.Services.Jobs;
using Fitz.Core.Services.Settings;
using Fitz.Features.Accounts.Commands;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Features.Lottery.Create;
using Fitz.Features.Lottery.Jobs.Services;
using Fitz.Features.Lottery.Models;
using Fitz.Variables;
using Fitz.Variables.Channels;
using Fitz.Variables.Emojis;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Lottery.Jobs
{
    public class LotteryJob : ITimedJob
    {
        private readonly DiscordClient dClient;
        private readonly ILotteryService lotteryService;
        private readonly BotLog botLog;
        private readonly SettingsService settingsService;
        private readonly LotterySubscriptionHandler subscriptionHandler;
        private readonly LotteryNotificationService notificationService;

        public LotteryJob(
            DiscordClient dClient,
            ILotteryService lotteryService,
            BankService bankService,
            AccountService accountService,
            BotLog botLog,
            SettingsService settingsService)
        {
            this.dClient = dClient;
            this.lotteryService = lotteryService;
            this.botLog = botLog;
            this.settingsService = settingsService;
            subscriptionHandler = new LotterySubscriptionHandler(dClient, lotteryService, bankService, accountService, settingsService);
            notificationService = new LotteryNotificationService(dClient, lotteryService);
        }

        public ulong Emoji => LotteryEmojis.Lottery;

        public int Interval => 1;

        public async Task Execute()
        {
            try
            {
                botLog.Information(LogConsoleSettings.Jobs, LotteryEmojis.Lottery, $"Starting Lottery Job...");

                // Get Current Lottery
                Models.Lottery currentDrawing = lotteryService.GetCurrentLottery();

                // Get current settings
                Settings settings = settingsService.GetSettings();

                if (currentDrawing == null)
                {
                    // Start new lottery
                    await lotteryService.StartNewLotteryAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(settings.LotteryDuration), settings.BaseLotteryPool);
                    currentDrawing = lotteryService.GetCurrentLottery();
                    await subscriptionHandler.HandleLotterySubscriptions();
                }

                // If lottery is over
                if (currentDrawing.EndDate < DateTime.UtcNow)
                {
                    await HandleLotteryEnd(currentDrawing, settings);
                    currentDrawing = lotteryService.GetCurrentLottery();
                }

                await UpdateLotteryInfoChannel(currentDrawing);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred when running lottery job.");
            }
        }

        private async Task HandleLotteryEnd(Models.Lottery currentDrawing, Settings settings)
        {
            // Determine lottery winner(s)
            List<Winner> winners = await lotteryService.DecideWinners(currentDrawing);

            // End lottery
            await lotteryService.EndLotteryAsync(currentDrawing);

            if (winners.Count == 0 || winners == null)
            {
                // Start new lottery and Roll over the prize pool into the next lottery
                await lotteryService.StartNewLotteryAsync(
                    DateTime.UtcNow,
                    DateTime.UtcNow.AddDays(settings.LotteryDuration),
                    currentDrawing.Pool + settings.BaseLotteryPool);
            }
            else
            {
                foreach (Winner winner in winners)
                {
                    // DM the winner
                    await notificationService.MessageWinner(winner.UserId, currentDrawing);
                }

                // Start new lottery with new prize pool.
                await lotteryService.StartNewLotteryAsync(
                    DateTime.UtcNow,
                    DateTime.UtcNow.AddDays(settings.LotteryDuration),
                    settings.BaseLotteryPool);
            }

            // Check to see if there are any lottery subscribers
            await subscriptionHandler.HandleLotterySubscriptions();
        }

        private async Task UpdateLotteryInfoChannel(Models.Lottery currentDrawing)
        {
            try
            {
                Settings settings = settingsService.GetSettings();
                DiscordChannel lotteryChannel = await dClient.GetChannelAsync(Waterbear.LotteryInfo);
                DiscordEmbedBuilder lotteryEmbed = new()
                {
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        IconUrl = DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Ticket).Url,
                        Text = $"Lottery#{currentDrawing.Id} | Last Winning Ticket: {lotteryService.GetLastWinningTicket()}",
                    },
                    Color = new DiscordColor(52, 114, 53),
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    {
                        Url = DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Lottery).Url,
                    },
                    Title = $"Current Lottery Information",
                    Description = $"{DiscordEmoji.FromName(dClient, ":beer:")}Beer Pool: `{currentDrawing.Pool}` \n" +
                    $"{DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Ticket)}Total Tickets: `{(int)lotteryService.GetTotalTickets().Data}`\n" +
                    $"{DiscordEmoji.FromGuildEmote(dClient, AccountEmojis.Users)}Total Users: `{(int)lotteryService.GetTotalLotteryParticipant().Data}`\n" +
                    $"{DiscordEmoji.FromName(dClient, ":clock2:")}Time Left: `{(int)lotteryService.GetRemainingHoursUntilNextDrawing().Data}` Hrs\n" +
                    $"Ticket cost: `{settings.TicketCost}` beer\n" +
                    $"Max Tickets per user: `{settings.MaxTickets}`"
                };

                string winnerNames = string.Empty;
                List<Account> priorWinners = lotteryService.GetLastLotteryWinnerAccounts();
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

                IAsyncEnumerable<DiscordMessage> lotteryMessages = lotteryChannel.GetMessagesAsync();
                if (lotteryMessages.ToBlockingEnumerable().Count() == 0)
                {
                    await lotteryChannel.SendMessageAsync(embed: lotteryEmbed.Build());
                }
                await foreach (DiscordMessage message in lotteryMessages)
                {
                    if (message.Author.Id == dClient.CurrentUser.Id)
                    {
                        await message.ModifyAsync(content: "Use `/lottery 0` for help.", embed: lotteryEmbed.Build());
                        botLog.Information(LogConsoleSettings.Jobs, LotteryEmojis.Lottery, $"Finished Lottery Job");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred when updating lottery info channel.");
            }
        }
    }
}