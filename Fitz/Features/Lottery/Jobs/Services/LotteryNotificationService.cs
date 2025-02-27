using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Features.Accounts.Models;
using Fitz.Variables;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fitz.Features.Lottery.Jobs.Services
{
    public class LotteryNotificationService
    {
        private readonly DiscordClient dClient;
        private readonly ILotteryService lotteryService;

        public LotteryNotificationService(
            DiscordClient dClient,
            ILotteryService lotteryService)
        {
            this.dClient = dClient;
            this.lotteryService = lotteryService;
        }

        public async Task MessageWinner(ulong userId, Models.Lottery drawing)
        {
            if (userId == 0)
            {
                return;
            }

            List<Account> winners = lotteryService.GetLastLotteryWinnerAccounts();
            DiscordGuild guild = await dClient.GetGuildAsync(Guilds.Waterbear);
            DiscordMember member = await guild.GetMemberAsync(userId);

            if (member == null || member.IsBot)
            {
                return;
            }

            // DM The winner to let them know.
            DiscordDmChannel userDMChannel = await member.CreateDmChannelAsync();
            await userDMChannel.SendMessageAsync(embed: lotteryService.WinnerEmbed(dClient, drawing, winners, userId));
        }
    }
}