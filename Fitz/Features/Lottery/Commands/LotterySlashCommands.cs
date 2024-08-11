using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.ModalCommands;
using DSharpPlus.SlashCommands;
using Fitz.Core.Commands.Attributes;
using Fitz.Core.Models;
using Fitz.Core.Services.Settings;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Features.Lottery.Models;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Fitz.Features.Lottery.Commands
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public class LotterySlashCommands(DiscordClient dClient,
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
        public async Task Lottery(InteractionContext ctx)
        {
            int unique_id = 0;
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[4];

                for (int i = 0; i < 4; i++)
                {
                    rng.GetBytes(data);
                    unique_id = BitConverter.ToInt32(data, 0);
                    unique_id = Math.Abs(unique_id);
                }
            }
            // Get settings for lottery
            Settings settings = settingsService.GetSettings();

            Models.Lottery lottery = lotteryService.GetCurrentLottery();

            // Get account
            Account account = accountService.FindAccount(ctx.User.Id);

            // Get user tickets
            List<Ticket> userTickets = lotteryService.GetUserTickets(account).Data as List<Ticket>;
            userTickets ??= new List<Ticket>();

            DiscordButtonComponent cancelBtn = new(DiscordButtonStyle.Danger, $"lottery_cancel_{unique_id}", "Cancel", false);
            DiscordButtonComponent helpBtn = new(DiscordButtonStyle.Secondary, $"lottery_help_{unique_id}", "Help", false);

            bool userHasMaxTickets = userTickets.Count >= settings.MaxTickets;

            DiscordButtonComponent buyMaxTicketsBtn = new(DiscordButtonStyle.Success, $"lottery_max_tickets_{unique_id}", "Buy Max Tickets", userHasMaxTickets);
            DiscordButtonComponent buyXBtn = new(DiscordButtonStyle.Primary, $"lottery_buy_x_{unique_id}", "Buy X", userHasMaxTickets);

            if (account == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent("You need to run `/signup` before you can interact with the lottery."));
                return;
            }

            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("How many tickets would you like to purchase?")
                .AddEmbed(this.lotteryService.LotteryCommandEmbed(ctx.Client, lottery, settings, account, userTickets))
                .AddComponents(cancelBtn, helpBtn, buyXBtn, buyMaxTicketsBtn).AsEphemeral(true));

            ctx.Client.ComponentInteractionCreated += async (sender, args) =>
            {
                if (args.Id == $"lottery_help_{unique_id}")
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .ClearEmbeds().AddEmbed(this.lotteryService.LotteryHelpEmbed(ctx.Client, lottery, settings)));
                }
                if (args.Id == $"lottery_cancel_{unique_id}")
                {
                    await ctx.DeleteResponseAsync();
                }
                if (args.Id == $"lottery_max_tickets_{unique_id}")
                {
                    var buyTicketsResult = await this.lotteryService.BuyTicketsForUser(account, settings.MaxTickets);
                    if (!buyTicketsResult.Success)
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                                                       .WithContent(buyTicketsResult.Message));
                    }

                    userTickets = lotteryService.GetUserTickets(account).Data as List<Ticket>;

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .AddEmbed(this.lotteryService.LotteryInfoEmbed(ctx.Client, lottery, (int)this.lotteryService.GetRemainingHoursUntilNextDrawing().Data, userTickets)));
                }
                if (args.Id == $"lottery_buy_x_{unique_id}")
                {
                    await ctx.DeleteResponseAsync();
                    try
                    {
                        var modal = ModalBuilder.Create("buy_x_tickets")
                            .WithTitle("Buy X Tickets")
                            .AddComponents(new DiscordTextInputComponent("How many tickets would you like to purchase?", "tickets", "Number of tickets", required: true, style: DiscordTextInputStyle.Short, min_length: 1, max_length: 3));

                        await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, modal);
                    }
                    catch (Exception ex)
                    {
                        await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                                                       new DiscordInteractionResponseBuilder()
                                                                                  .WithContent("An error occurred while buying tickets. Please try again later."));
                    }
                }
            };
        }

        [SlashCommand("buyTickets", "Play stupid games. Win beer. Lose beer.")]
        [RequireAccount]
        public async Task LotteryTickets(InteractionContext ctx, [Option("Tickets", "How many tickets do you want?")] long tickets)
        {
            Settings settings = settingsService.GetSettings();
            Account account = accountService.FindAccount(ctx.User.Id);
            if (account == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent("You need to run `/signup` before you can interact with the lottery."));
                return;
            }

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

            //using var qrGenerator = new QRCodeGenerator();
            //using var qrCodeData = qrGenerator.CreateQrCode(ticketNumbers, QRCodeGenerator.ECCLevel.Q);
            //PngByteQRCode qrCodeImage = new(qrCodeData);
            //using MemoryStream ms = new(qrCodeImage.GetGraphic(5, false));
            //DiscordInteractionResponseBuilder responseBuilder = new();
            //responseBuilder.AddFile("qrCode.png", ms);
            //responseBuilder.AddEmbed(lotteryEmbed(account, lottery, daysLeft, userTickets)).AsEphemeral(true);

            //DiscordWebhookBuilder webhookBuilder = new();
            //webhookBuilder.AddFile("qrCode.png", ms);
            //webhookBuilder.AddEmbed(lotteryEmbed(account, lottery, daysLeft, userTickets));

            //await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, responseBuilder);

            //await ctx.EditResponseAsync(webhookBuilder);
        }

        #endregion Lottery

        #region Lottery Info

        [SlashCommand("lotteryinfo", "Get information about the current lottery.")]
        [RequireAccount]
        public async Task LotteryInfo(InteractionContext ctx)
        {
            //Models.Lottery drawing = lotteryService.GetCurrentLottery();
            //List<Ticket> userTickets = new();
            //Account account = accountService.FindAccount(ctx.User.Id);
            //int daysLeft = (int)this.lotteryService.GetRemainingHoursUntilNextDrawing().Data;
            //userTickets = lotteryService.GetUserTickets(accountService.FindAccount(ctx.User.Id)).Data as List<Ticket>;
            //string ticketNumbers = string.Empty;
            //foreach (var ticket in userTickets)
            //{
            //    ticketNumbers += $"{ticket.Number}\n";
            //}

            //using var qrGenerator = new QRCodeGenerator();
            //using var qrCodeData = qrGenerator.CreateQrCode(ticketNumbers, QRCodeGenerator.ECCLevel.Q);
            //PngByteQRCode qrCodeImage = new(qrCodeData);

            //using MemoryStream ms = new MemoryStream(qrCodeImage.GetGraphic(5, false));
            //DiscordInteractionResponseBuilder responseBuilder = new();
            //responseBuilder.AddFile("qrCode.png", ms);
            //responseBuilder.AddEmbed(lotteryEmbed(account, drawing, daysLeft, userTickets)).AsEphemeral(true);

            //await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, responseBuilder);
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
    }
}