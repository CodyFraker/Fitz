using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fitz.Core.Contexts;
using Fitz.Features.Lottery.Models;

namespace Fitz.Features.Lottery
{
    internal class LotteryAdminCommands : BaseCommandModule
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
            // Check if there is a current lottery
            Drawing drawing = await lotteryService.GetCurrentLottery();
            if (drawing == null)
            {
                await ctx.RespondAsync("There is no current lottery.");
            }
            else
            {
                // End the current lottery
                lotteryService.EndLottery().Wait();
                await ctx.RespondAsync("The current lottery has ended.");
            }
        }
    }
}