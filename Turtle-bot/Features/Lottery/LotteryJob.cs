using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Core.Discord;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Features.Lottery.Models;
using Fitz.Variables;
using Fitz.Variables.Channels;
using Fitz.Variables.Emojis;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Lottery
{
    public class LotteryJob : ITimedJob
    {
        private readonly DiscordClient dClient;
        private readonly LotteryService lotteryService;
        private readonly BankService bankService;

        private readonly BotLog botLog;

        public LotteryJob(DiscordClient dClient, LotteryService lotteryService, BankService bankService, BotLog botLog)
        {
            this.dClient = dClient;
            this.lotteryService = lotteryService;
            this.bankService = bankService;
            this.botLog = botLog;
        }

        public ulong Emoji => LotteryEmojis.Lottery;

        public int Interval => 1;

        private int Pool = 36;

        private int DaysToRunLottery = 1;

        public async Task Execute()
        {
            try
            {
                this.botLog.Information(LogConsoleSettings.Jobs, LotteryEmojis.Lottery, $"Starting Lottery Job");
                // Get Current Lottery
                Drawing currentDrawing = this.lotteryService.GetCurrentDrawing();

                if (currentDrawing == null)
                {
                    // Start new lottery
                    await this.lotteryService.StartNewLotteryAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(DaysToRunLottery), Pool);
                    currentDrawing = this.lotteryService.GetCurrentDrawing();
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
                    Description = $"{DiscordEmoji.FromName(this.dClient, ":beer:")}**BEER POOL**: `{currentDrawing.Pool}` \n" +
                    $"{DiscordEmoji.FromName(this.dClient, ":ticket:")} Total Tickets: `{await lotteryService.GetTotalTickets()}`\n" +
                    $"{DiscordEmoji.FromGuildEmote(this.dClient, LotteryEmojis.User)}Total Users: `{await lotteryService.GetTotalLotteryParticipant()}`\n" +
                    $"{DiscordEmoji.FromName(this.dClient, ":clock2:")}Time Left: `{await this.lotteryService.GetRemainingHoursUntilNextDrawing()} Hrs`",
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

        private async Task MessageWinner(ulong userId, Drawing drawing)
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