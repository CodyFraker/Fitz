using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Core.Commands.Attributes;
using Fitz.Core.Models;
using Fitz.Core.Services.Settings;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Features.Lottery.Models;
using Fitz.Variables.Emojis;
using QRCoder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace Fitz.Features.Lottery.Commands
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    internal class LotterySlashCommands(DiscordClient dClient,
        AccountService accountService,
        BankService bankService,
        LotteryService lotteryService,
        SettingsService settingsService) : ApplicationCommandModule
    {
        private readonly DiscordClient dClient = dClient;
        private readonly AccountService accountService = accountService;
        private readonly BankService bankService = bankService;
        private readonly LotteryService lotteryService = lotteryService;
        private readonly SettingsService settingsService = settingsService;

        #region Lottery

        [SlashCommand("lottery", "Play stupid games. Win beer. Lose beer.")]
        [RequireAccount]
        public async Task Lottery(InteractionContext ctx, [Option("Tickets", "How many tickets do you want?")] long tickets = 0)
        {
            await ctx.DeferAsync(true);
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

                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                                       .AddEmbed(lotteryHelpEmbed.Build()));
                return;
            }

            Account account = accountService.FindAccount(ctx.User.Id);
            if (account == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent("You need to run `/signup` before you can interact with the lottery."));
                return;
            }

            Settings settings = settingsService.GetSettings();

            // Check if user is trying to buy too many tickets
            if (tickets > settings.MaxTickets)
            {
                DiscordButtonComponent cancelBtn = new(DiscordButtonStyle.Danger, $"lottery_cancel", "Cancel", false);
                DiscordButtonComponent buyMaxTicketsBtn = new(DiscordButtonStyle.Success, $"lottery_max_tickets", "Buy Max Tickets", false);

                await ctx.DeferAsync(true);
                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder()
                    .WithContent($"Do you wish to purchase the max amount of tickets.")
                    .AddComponents(cancelBtn, buyMaxTicketsBtn));
            }

            int totalCost = (int)(tickets * settings.TicketCost);

            // Check if user has enough beer
            if (account.Beer < totalCost)
            {
                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder()
                    .WithContent("You don't have enough beer to buy that many tickets."));
                return;
            }

            // If the user has already bought settings.MaxTickets tickets for the current lottery, don't let them buy more
            List<Ticket> userTickets = lotteryService.GetUserTickets(account).Data as List<Ticket>;
            userTickets ??= new List<Ticket>();
            if (userTickets.Count + tickets > settings.MaxTickets)
            {
                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder()
                    .WithContent($"You can only buy up to {settings.MaxTickets} tickets for each lottery. Check your tickets using `/lotteryinfo`"));
                return;
            }

            // Deduct the cost of the tickets from user's "bank"
            await bankService.PurchaseLotteryTicket(account, totalCost);

            await lotteryService.CreateTicket(account, (int)tickets);

            await lotteryService.AddToPool(totalCost);

            // Recall service to get updated lottery ticket info.
            userTickets = lotteryService.GetUserTickets(account).Data as List<Ticket>;

            string ticketNumbers = string.Empty;
            foreach (var ticket in userTickets)
            {
                ticketNumbers += $"{ticket.Number}\n";
            }

            Models.Lottery lottery = lotteryService.GetCurrentLottery();
            int daysLeft = (int)this.lotteryService.GetRemainingHoursUntilNextDrawing().Data;

            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(ticketNumbers, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCodeImage = new(qrCodeData);
            using MemoryStream ms = new(qrCodeImage.GetGraphic(5, false));
            DiscordInteractionResponseBuilder responseBuilder = new();
            responseBuilder.AddFile("qrCode.png", ms);
            responseBuilder.AddEmbed(lotteryEmbed(account, lottery, daysLeft, userTickets)).AsEphemeral(true);

            DiscordWebhookBuilder webhookBuilder = new();
            webhookBuilder.AddFile("qrCode.png", ms);
            webhookBuilder.AddEmbed(lotteryEmbed(account, lottery, daysLeft, userTickets));

            //await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, responseBuilder);
            await ctx.EditResponseAsync(webhookBuilder);
        }

        #endregion Lottery

        #region Lottery Info

        [SlashCommand("lotteryinfo", "Get information about the current lottery.")]
        [RequireAccount]
        public async Task LotteryInfo(InteractionContext ctx)
        {
            Models.Lottery drawing = lotteryService.GetCurrentLottery();
            List<Ticket> userTickets = new();
            Account account = accountService.FindAccount(ctx.User.Id);
            int daysLeft = (int)this.lotteryService.GetRemainingHoursUntilNextDrawing().Data;
            userTickets = lotteryService.GetUserTickets(accountService.FindAccount(ctx.User.Id)).Data as List<Ticket>;
            string ticketNumbers = string.Empty;
            foreach (var ticket in userTickets)
            {
                ticketNumbers += $"{ticket.Number}\n";
            }

            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(ticketNumbers, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCodeImage = new(qrCodeData);

            using MemoryStream ms = new MemoryStream(qrCodeImage.GetGraphic(5, false));
            DiscordInteractionResponseBuilder responseBuilder = new();
            responseBuilder.AddFile("qrCode.png", ms);
            responseBuilder.AddEmbed(lotteryEmbed(account, drawing, daysLeft, userTickets)).AsEphemeral(true);

            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, responseBuilder);
        }

        #endregion Lottery Info

        #region My Tickets

        [SlashCommand("mytickets", "Get a list of your current lottery tickets.")]
        [RequireAccount]
        public async Task MyTickets(InteractionContext ctx)
        {
            Account account = accountService.FindAccount(ctx.User.Id);
            Models.Lottery drawing = lotteryService.GetCurrentLottery();

            if (lotteryService.GetUserTickets(account).Data is not List<Ticket> userTickets || userTickets.Count == 0)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                                       new DiscordInteractionResponseBuilder()
                                       .WithContent("You don't have any tickets for the current lottery.")
                                       .AsEphemeral(true));
                return;
            }

            string ticketNumbers = string.Empty;
            foreach (var ticket in userTickets)
            {
                ticketNumbers += $"{DiscordEmoji.FromName(ctx.Client, ":ticket:")}{ticket.Number}, ";
            }
            DiscordEmbedBuilder lotteryEmbed = new()
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(ctx.Client, LotteryEmojis.Ticket).Url,
                    Text = $"Lottery #{drawing.Id}",
                },
                Color = new DiscordColor(52, 114, 53),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = DiscordEmoji.FromGuildEmote(ctx.Client, LotteryEmojis.Lottery).Url,
                },
                Title = $"Your lottery tickets:",
                Timestamp = DateTime.UtcNow,
                Description = $"{ticketNumbers}",
            };

            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                               new DiscordInteractionResponseBuilder()
                                              .AddEmbed(lotteryEmbed.Build())
                                                             .AsEphemeral(true));
        }

        #endregion My Tickets

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
                        Text = $"Lottery #{lottery.Id} Time Left: {daysLeft} Hrs",
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
                        IconUrl = DiscordEmoji.FromGuildEmote(this.dClient, LotteryEmojis.Ticket).Url,
                        Text = $"Lottery #{lottery.Id} | Time Left: {daysLeft} Hrs",
                    },
                    Color = new DiscordColor(52, 114, 53),
                    //Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    //{
                    //    Url = DiscordEmoji.FromGuildEmote(ctx.Client, LotteryEmojis.Lottery).Url,
                    //},
                    Title = $"Current Lottery Information",
                    Description = $"**__Your Entries__**: ```ansi\n\u001b[1;37m{userTickets.Count}\u001b[0;0m\n```\n" +
                    $"Your tickets are stored in the QR code.\n" +
                    $"To see their values, run `/mytickets`"
                };
                lotteryEmbed.WithThumbnail(url: $"attachment://qrCode.png");
                lotteryEmbed.AddField($"**{DiscordEmoji.FromName(this.dClient, ":beer:")}Fridge**", $"```ansi\n\u001b[0;36m{lottery.Pool}\u001b[0;0m\n```", true);
                lotteryEmbed.AddField($"**{DiscordEmoji.FromGuildEmote(this.dClient, AccountEmojis.Users)}Participants**", $"```ansi\n\u001b[0;36m{(int)this.lotteryService.GetTotalLotteryParticipant().Data}\u001b[0;0m\n```", true);
                lotteryEmbed.AddField($"**{DiscordEmoji.FromName(this.dClient, ":ticket:")}Entries**", $"```ansi\n\u001b[0;36m{(int)this.lotteryService.GetTotalTickets().Data}\u001b[0;0m\n```", true);

                lotteryEmbed.AddField($"**Starts**", $"```ansi\n\u001b[1;33m{lottery.StartDate}\u001b[0;0m\n```", false);
                lotteryEmbed.AddField($"**Ends**", $"```ansi\n\u001b[1;31m{lottery.EndDate}\u001b[0;0m\n```", false);
            }

            return lotteryEmbed.Build();
        }

        #endregion Embeds
    }
}