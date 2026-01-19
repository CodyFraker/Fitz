using DSharpPlus.Entities;
using DSharpPlus.ModalCommands;
using DSharpPlus.ModalCommands.Attributes;
using Fitz.Database.Entities;
using Fitz.Database.Entities;
using Fitz.Features.Accounts;
using System.Threading.Tasks;
using DSharpPlus;
using Fitz.Features.Bank;
using Fitz.Database.Entities;
using Fitz.Features.Settings;
using System.Collections.Generic;

namespace Fitz.Features.Lottery.Commands
{
    public class LotteryModalCommands(DiscordClient dClient,
        AccountService accountService,
        BankService bankService,
        LotteryService lotteryService,
        SettingsService settingsService) : ModalCommandModule
    {
        private readonly DiscordClient dClient = dClient;
        private readonly AccountService accountService = accountService;
        private readonly BankService bankService = bankService;
        private readonly LotteryService lotteryService = lotteryService;
        private readonly SettingsService settingsService = settingsService;

        [ModalCommand("buy_x_tickets")]
        public async Task LotteryBuyXTickets(ModalContext ctx, string tickets)
        {
            try
            {
                if (!int.TryParse(tickets, out int ticketCount) || ticketCount <= 0)
                {
                    await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                                               new DiscordInteractionResponseBuilder()
                                                                      .WithContent("Please enter a valid number of tickets to buy.").AsEphemeral(true));
                    return;
                }

                Fitz.Database.Entities.Settings settings = settingsService.GetSettings();

                Database.Entities.LotteryEntity lottery = lotteryService.GetCurrentLottery();

                // Get account
                AccountEntity account = accountService.FindAccount(ctx.User.Id);
                if (account == null)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent("You need to run `/signup` before you can interact with the lottery."));
                    return;
                }

                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("Generating tickets...").AsEphemeral(true));

                var buyTicketResult = await lotteryService.BuyTicketsForUser(account, int.Parse(tickets));

                if (!buyTicketResult.Success)
                {
                    await ctx.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                                           .WithContent(buyTicketResult.Message));
                    return;
                }
                else
                {
                    List<Ticket> userTickets = buyTicketResult.Data as List<Ticket>;

                    int daysLeft = (int)this.lotteryService.GetRemainingHoursUntilNextDrawing().Data;

                    await ctx.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                        .WithContent("Tickets purchased successfully.")
                                       .AddEmbed(this.lotteryService.LotteryInfoEmbed(ctx.Client, lottery, daysLeft, userTickets)));
                }
            }
            catch (System.Exception ex)
            {
                await ctx.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                                       .WithContent("An error occurred while buying tickets. Please try again later."));
            }
        }
    }
}