using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Features.Accounts;
using Fitz.Features.Bank;
using System.Threading.Tasks;
using Fitz.Core.Contexts;
using Fitz.Variables.Emojis;
using DSharpPlus.ModalCommands;
using System;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Lottery.Models;
using System.Collections.Generic;

namespace Fitz.Features.Lottery
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    internal class LotterySlashCommands : ApplicationCommandModule
    {
        private readonly BotContext db;
        private readonly AccountService accountService;
        private readonly BankService bankService;
        private readonly LotteryService lotteryService;
        private const int TicketCost = 1;

        public LotterySlashCommands(BotContext db, LotteryService lotteryService, AccountService accountService, BankService bankService)
        {
            this.db = db;
            this.accountService = accountService;
            this.bankService = bankService;
            this.lotteryService = lotteryService;
        }

        [SlashCommand("lottery", "Play stupid games. Win beer. Lose beer. 1 ticket = 1 beer.")]
        public async Task Lottery(InteractionContext ctx, [Option("Tickets", "How many tickets do you want? (max: 10)")] long tickets)
        {
            // Check if user has an account
            Accounts.Models.Account user = accountService.FindAccount(ctx.User.Id);
            if (user == null)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You need to run `/signup` before you can interact with the lottery.").AsEphemeral(true));
                return;
            }

            // Check if user is trying to buy too many tickets
            if (tickets > 10)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You can only buy up to 10 tickets at a time.").AsEphemeral(true));
                return;
            }

            int totalCost = (int)(tickets * TicketCost);

            // Check if user has enough beer
            if (user.Beer < totalCost)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You don't have enough beer to buy that many tickets.").AsEphemeral(true));
                return;
            }

            // If the user has already bought 10 tickets for the current lottery, don't let them buy more
            var userTickets = await lotteryService.GetUserTickets(user);
            if (userTickets.Count + tickets > 10)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You can only buy up to 10 tickets for each lottery. Check your tickets using `/lotteryinfo`").AsEphemeral(true));
                return;
            }

            // Deduct the cost of the tickets from user's "bank"
            await bankService.PurchaseLotteryTicket(user, totalCost);

            await lotteryService.CreateTicket(user, (int)tickets);

            await lotteryService.AddToPool(totalCost);

            // Recall service to get updated lottery ticket info.
            userTickets = await lotteryService.GetUserTickets(user);

            string ticketNumbers = string.Empty;
            foreach (var ticket in userTickets)
            {
                ticketNumbers += $"{ticket.Number}\n";
            }

            DiscordEmbedBuilder lotteryTicketEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(ctx.Client, LotteryEmojis.Ticket).Url,
                    Text = $"Lottery #{await lotteryService.GetCurrentDrawingId()} | Prize Pool: {await this.lotteryService.GetCurrentPrizePool()}",
                },
                Color = new DiscordColor(52, 114, 53),
                Timestamp = DateTime.UtcNow,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = DiscordEmoji.FromGuildEmote(ctx.Client, LotteryEmojis.Lottery).Url
                },
                Description = $"Tickets:\n" +
                    $"{ticketNumbers}",
            };
            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(lotteryTicketEmbed.Build()).AsEphemeral(true));
        }

        [SlashCommand("lotteryinfo", "Get information about the current lottery.")]
        public async Task LotteryInfo(InteractionContext ctx)
        {
            Drawing drawing = await lotteryService.GetCurrentLottery();
            int totalTickets = await lotteryService.GetTotalTickets();
            List<Ticket> userTickets = new List<Ticket>();
            userTickets = await lotteryService.GetUserTickets(accountService.FindAccount(ctx.User.Id));
            string ticketNumbers = string.Empty;
            foreach (var ticket in userTickets)
            {
                ticketNumbers += $"{ticket.Number}\n";
            }

            DiscordEmbedBuilder lotteryEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(ctx.Client, LotteryEmojis.Ticket).Url,
                    Text = $"Lottery | Last Winner: Fitz",
                },
                Color = new DiscordColor(52, 114, 53),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = DiscordEmoji.FromGuildEmote(ctx.Client, LotteryEmojis.Lottery).Url,
                },
                Title = $"Current Lottery Information",
                Description = $"**PRIZE POOL**: {drawing.Pool} \n" +
                $"Total Tickets: {await lotteryService.GetTotalTickets()}\n" +
                $"Total Users: {await lotteryService.GetTotalLotteryParticipant()}",
                Url = "",
            };

            lotteryEmbed.AddField($"**Starts**", $"`{drawing.StartDate}`", true);
            lotteryEmbed.AddField($"**Ends**", $"`{drawing.EndDate}`", true);
            lotteryEmbed.AddField($"**Your Tickets**", $"`{ticketNumbers}`", false);

            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(lotteryEmbed.Build()).AsEphemeral(true));
        }
    }
}