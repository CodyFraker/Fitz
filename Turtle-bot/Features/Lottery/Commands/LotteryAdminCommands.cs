using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Fitz.Core.Contexts;
using Fitz.Features.Lottery.Attributes;
using Fitz.Features.Lottery.Models;
using Fitz.Variables.Emojis;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Lottery.Commands
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    [RequireLotteryAdmin]
    public class LotteryAdminCommands : BaseCommandModule
    {
        private readonly BotContext db;
        private readonly LotteryService lotteryService;

        public LotteryAdminCommands(BotContext db, LotteryService lotteryService)
        {
            this.db = db;
            this.lotteryService = lotteryService;
        }

        [Command("createlottery")]
        [Description("Creates a lottery.")]
        public Task CreateNewLottery(CommandContext ctx)
        {
            return ctx.RespondAsync("beer.");
        }

        [Command("endlottery")]
        [Description("Ends a lottery.")]
        public async Task StopCurrentLottery(CommandContext ctx)
        {
            Drawing drawing = lotteryService.GetCurrentDrawing();
            if (drawing == null)
            {
                await ctx.RespondAsync("There is no active lottery.");
                return;
            }

            await lotteryService.UpdateCurrentLottery(endDate: DateTime.UtcNow);
            await ctx.RespondAsync("Set current lottery to end next job cycle.");
        }

        [Command("mockwin")]
        [Description("Sends you a DM that looks like you won the current lottery.")]
        public async Task MockWinnerLottery(CommandContext ctx)
        {
            DiscordGuild guild = ctx.Guild;
            DiscordMember member = await guild.GetMemberAsync(ctx.User.Id);
            if (member == null || member.IsBot)
            {
                return;
            }
            // DM The winner to let them know.
            DiscordDmChannel userDMChannel = await member.CreateDmChannelAsync();

            Drawing drawing = new Drawing()
            {
                Id = 138,
                Pool = 36,
                WinningTicket = 1,
            };

            DiscordEmbedBuilder lotteryEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(ctx.Client, LotteryEmojis.Ticket).Url,
                    Text = $"Lottery | #{drawing.Id}",
                },
                Color = new DiscordColor(52, 114, 53),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = DiscordEmoji.FromGuildEmote(ctx.Client, LotteryEmojis.Lottery).Url,
                },
                Title = $"Congratulations! You've won lottery#{drawing.Id}!",
                Timestamp = DateTime.UtcNow,
                Description = $"The prize pool of {drawing.Pool} beer is now yours. \n" +
                $"New beer balance: `2833`",
            };
            lotteryEmbed.AddField($"**Total Tickets**", $"`234`", true);
            lotteryEmbed.AddField($"**Total Users**", $"`14`", true);

            await userDMChannel.SendMessageAsync(embed: lotteryEmbed.Build());
        }
    }
}