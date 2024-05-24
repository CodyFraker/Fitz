using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Core.Commands.Attributes;
using Fitz.Core.Contexts;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Features.Lottery.Models;
using Fitz.Variables.Emojis;
using QRCoder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Fitz.Features.Lottery.Commands
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    internal class LotterySlashCommands : ApplicationCommandModule
    {
        private readonly BotContext db;
        private readonly DiscordClient dClient;
        private readonly AccountService accountService;
        private readonly BankService bankService;
        private readonly LotteryService lotteryService;
        private const int TicketCost = 1;

        public LotterySlashCommands(BotContext db, LotteryService lotteryService, AccountService accountService, DiscordClient dClient, BankService bankService)
        {
            this.db = db;
            this.dClient = dClient;
            this.accountService = accountService;
            this.bankService = bankService;
            this.lotteryService = lotteryService;
        }

        [SlashCommand("lottery", "Play stupid games. Win beer. Lose beer. 1 ticket = 1 beer.")]
        [RequireAccount]
        public async Task Lottery(InteractionContext ctx, [Option("Tickets", "How many tickets do you want? (max: 36)")] long tickets = 0)
        {
            // If no tickets are specified, send help embed.
            if (tickets == 0)
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
                        Url = DiscordEmoji.FromGuildEmote(ctx.Client, LotteryEmojis.Lottery).Url
                    },
                };

                lotteryHelpEmbed.AddField($"Commands",
                    $"`/lottery #` will buy a set amount of tickets. Providing 0 tickets will return this message again.\n" +
                    $"\n" +
                    $"`/lotteryinfo` will show you some basic information about the current drawing. The QR code will show you which tickets you have in this drawing.\n" +
                    $"\n" +
                    $"You can set your account to automatically play the lottery for by doing `/settings`.", false);

                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(lotteryHelpEmbed.Build()).AsEphemeral(true));
                return;
            }

            Account account = accountService.FindAccount(ctx.User.Id);
            if (account == null)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("You need to run `/signup` before you can interact with the lottery.")
                    .AsEphemeral(true));
                return;
            }

            // Check if user is trying to buy too many tickets
            if (tickets > 36)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("You can only buy up to 36 tickets at a time.")
                    .AsEphemeral(true));
                return;
            }

            int totalCost = (int)(tickets * TicketCost);

            // Check if user has enough beer
            if (account.Beer < totalCost)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("You don't have enough beer to buy that many tickets.")
                    .AsEphemeral(true));
                return;
            }

            // If the user has already bought 36 tickets for the current lottery, don't let them buy more
            var userTickets = lotteryService.GetUserTickets(account);
            if (userTickets.Count + tickets > 36)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("You can only buy up to 36 tickets for each lottery. Check your tickets using `/lotteryinfo`")
                    .AsEphemeral(true));
                return;
            }

            // Deduct the cost of the tickets from user's "bank"
            await bankService.PurchaseLotteryTicket(account, totalCost);

            await lotteryService.CreateTicket(account, (int)tickets);

            await lotteryService.AddToPool(totalCost);

            // Recall service to get updated lottery ticket info.
            userTickets = lotteryService.GetUserTickets(account);

            string ticketNumbers = string.Empty;
            foreach (var ticket in userTickets)
            {
                ticketNumbers += $"{DiscordEmoji.FromName(ctx.Client, ":ticket:")}{ticket.Number}\n";
            }

            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(ticketNumbers, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCodeImage = new PngByteQRCode(qrCodeData);

            //using (MemoryStream ms = new MemoryStream(qrCodeImage.GetGraphic(2, false)))
            //{
            //    DiscordInteractionResponseBuilder responseBuilder = new DiscordInteractionResponseBuilder();
            //    responseBuilder.AddFile("qrCode.png", ms);
            //    responseBuilder.AddEmbed(lotteryEmbed(account, drawing, daysLeft, userTickets)).AsEphemeral(true);

            //    await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, responseBuilder);
            //}
        }

        [SlashCommand("lotteryinfo", "Get information about the current lottery.")]
        [RequireAccount]
        public async Task LotteryInfo(InteractionContext ctx)
        {
            Models.Lottery drawing = lotteryService.GetCurrentDrawing();
            List<Ticket> userTickets = new List<Ticket>();
            Account account = accountService.FindAccount(ctx.User.Id);
            int daysLeft = await this.lotteryService.GetRemainingHoursUntilNextDrawing();
            userTickets = lotteryService.GetUserTickets(accountService.FindAccount(ctx.User.Id));
            string ticketNumbers = string.Empty;
            foreach (var ticket in userTickets)
            {
                ticketNumbers += $"{ticket.Number}\n";
            }

            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(ticketNumbers, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCodeImage = new PngByteQRCode(qrCodeData);

            using (MemoryStream ms = new MemoryStream(qrCodeImage.GetGraphic(2, false)))
            {
                DiscordInteractionResponseBuilder responseBuilder = new DiscordInteractionResponseBuilder();
                responseBuilder.AddFile("qrCode.png", ms);
                responseBuilder.AddEmbed(lotteryEmbed(account, drawing, daysLeft, userTickets)).AsEphemeral(true);

                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, responseBuilder);
            }
        }

        #region Embeds

        private DiscordEmbed lotteryEmbed(Account account, Models.Lottery lottery, int daysLeft, List<Ticket> userTickets = null)
        {
            DiscordEmbedBuilder lotteryEmbed;
            if (userTickets == null)
            {
                lotteryEmbed = new DiscordEmbedBuilder
                {
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        IconUrl = DiscordEmoji.FromGuildEmote(this.dClient, LotteryEmojis.Lottery).Url,
                        // TODO: Remove Fitz as the last winner and replace with the actual last winner.
                        Text = $"Lottery #{lottery.Id} | Last Winner: Fitz | Time Left: {daysLeft} Hrs",
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
                        IconUrl = DiscordEmoji.FromGuildEmote(this.dClient, LotteryEmojis.Lottery).Url,
                        // TODO: Remove Fitz as the last winner and replace with the actual last winner.
                        Text = $"Lottery #{lottery.Id} | Last Winner: Fitz | Time Left: {daysLeft} Hrs",
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
                lotteryEmbed.WithThumbnail(url: $"attachment://qrCode.png");
                lotteryEmbed.AddField($"**{DiscordEmoji.FromName(this.dClient, ":beer:")}Fridge**", $"```{lottery.Pool}```", true);
                lotteryEmbed.AddField($"**{DiscordEmoji.FromGuildEmote(this.dClient, LotteryEmojis.User)}Participants**", $"```45```", true);
                lotteryEmbed.AddField($"**{DiscordEmoji.FromName(this.dClient, ":ticket:")}Entries**", $"```45```", true);

                lotteryEmbed.AddField($"**Starts**", $"```{lottery.StartDate}```", false);
                lotteryEmbed.AddField($"**Ends**", $"```{lottery.EndDate}```", false);
            }

            return lotteryEmbed.Build();
        }

        #endregion Embeds
    }
}