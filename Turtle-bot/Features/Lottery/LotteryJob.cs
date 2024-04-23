using DSharpPlus;
using Fitz.Core.Discord;
using Fitz.Core.Services.Jobs;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fitz.Variables;
using Serilog;
using DSharpPlus.Entities;
using Fitz.Variables.Channels;
using Fitz.Features.Lottery.Models;
using Microsoft.VisualBasic;
using Fitz.Features.Accounts.Models;

namespace Fitz.Features.Lottery
{
    public class LotteryJob : ITimedJob
    {
        private readonly DiscordClient dCLient;
        private readonly LotteryService lotteryService;

        public LotteryJob(DiscordClient dCLient, LotteryService lotteryService)
        {
            this.dCLient = dCLient;
            this.lotteryService = lotteryService;
        }

        public ulong Emoji => LotteryEmojis.Lottery;

        public int Interval => 5;

        public async Task Execute()
        {
            Log.Information("Starting lottery job.");
            DiscordChannel lotteryChannel = await this.dCLient.GetChannelAsync(Channels.LotteryInfo);

            Drawing drawing = await this.lotteryService.GetCurrentLottery();

            DiscordEmbedBuilder lotteryEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(this.dCLient, LotteryEmojis.Ticket).Url,
                    Text = $"Lottery | Last Winner: Fitz",
                },
                Color = new DiscordColor(52, 114, 53),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = DiscordEmoji.FromGuildEmote(this.dCLient, LotteryEmojis.Lottery).Url,
                },
                Title = $"Current Lottery Information",
                Description = $"**PRIZE POOL**: {drawing.Pool} \n" +
                $"Total Tickets: {await lotteryService.GetTotalTickets()}\n" +
                $"Total Users: {await lotteryService.GetTotalLotteryParticipant()}",
                Url = "",
            };

            lotteryEmbed.AddField($"**Starts**", $"`{drawing.StartDate}`", true);
            lotteryEmbed.AddField($"**Ends**", $"`{drawing.EndDate}`", true);
            try
            {
                IAsyncEnumerable<DiscordMessage> lotteryMessages = lotteryChannel.GetMessagesAsync();
                await foreach (DiscordMessage message in lotteryMessages)
                {
                    if (message.Author.Id == this.dCLient.CurrentUser.Id)
                    {
                        await message.ModifyAsync(content: "Use `/lottery` to get started.", embed: lotteryEmbed.Build());
                        return;
                    }
                }

                Log.Information("Finished lottery job.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occured when running lottery job.");
            }
        }
    }
}