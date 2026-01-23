using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.ModalCommands;
using DSharpPlus.SlashCommands;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Lottery.BuyTickets.Domain;
using Fitz.Api.Controllers.Lottery.Embeds;
using Fitz.Api.Controllers.Lottery.Exceptions;
using Fitz.Api.Controllers.Lottery.GetCurrentLottery.Domain;
using Fitz.Api.Controllers.Lottery.Interactions;
using System.Security.Cryptography;

namespace Fitz.Api.Controllers.Lottery.GetCurrentLottery.Discord;

[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public sealed class GetCurrentLotterySlashCommand(
    GetCurrentLotteryFacade getCurrentLotteryFacade,
    IGetCurrentLottery getCurrentLottery,
    IBuyTickets buyTickets,
    BuyTicketsFacade buyTicketsFacade,
    DiscordClient discordClient,
    ILogger<GetCurrentLotterySlashCommand> logger) : ApplicationCommandModule
{
    private readonly GetCurrentLotteryFacade _getCurrentLotteryFacade = getCurrentLotteryFacade;
    private readonly IGetCurrentLottery _getCurrentLottery = getCurrentLottery;
    private readonly IBuyTickets _buyTickets = buyTickets;
    private readonly BuyTicketsFacade _buyTicketsFacade = buyTicketsFacade;
    private readonly DiscordClient _discordClient = discordClient;
    private readonly ILogger<GetCurrentLotterySlashCommand> _logger = logger;

    [SlashCommand("lottery", "Play stupid games. Win beer. Lose beer.")]
    public async Task Lottery(InteractionContext ctx)
    {
        var userId = ctx.User.Id;
        var username = ctx.User.Username;

        _logger.LogInformation("Lottery command started via Discord slash command. UserId: {UserId}, Username: {Username}", userId, username);

        int uniqueId = 0;
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            byte[] data = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                rng.GetBytes(data);
                uniqueId = BitConverter.ToInt32(data, 0);
                uniqueId = Math.Abs(uniqueId);
            }
        }

        try
        {
            var lotteryCommand = GetCurrentLotteryCommand.From();
            var lotteryResponse = await _getCurrentLotteryFacade.Execute(lotteryCommand, CancellationToken.None);

            var settings = await _buyTickets.GetSettingsAsync(CancellationToken.None);
            if (settings == null)
            {
                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("Failed to retrieve lottery settings. Please try again later.")
                    .AsEphemeral(true));
                return;
            }

            var account = await _buyTickets.FindAccountByIdAsync(userId, CancellationToken.None);
            if (account == null)
            {
                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("You need to run `/signup` before you can interact with the lottery.")
                    .AsEphemeral(true));
                return;
            }

            var userTickets = await _buyTickets.GetUserTicketsAsync(userId, lotteryResponse.Id, CancellationToken.None);
            userTickets ??= new List<Fitz.Database.Entities.TicketEntity>();

            int remainingHours = (int)(lotteryResponse.EndDate - DateTime.UtcNow).TotalHours;
            int lastWinningTicket = await _getCurrentLottery.GetLastWinningTicketAsync(CancellationToken.None);

            var cancelBtn = LotteryButtons.CreateCancelButton(uniqueId);
            var helpBtn = LotteryButtons.CreateHelpButton(uniqueId);
            bool userHasMaxTickets = userTickets.Count >= settings.MaxTickets;
            var buyMaxTicketsBtn = LotteryButtons.CreateBuyMaxTicketsButton(uniqueId, userHasMaxTickets);
            var buyXBtn = LotteryButtons.CreateBuyXButton(uniqueId, userHasMaxTickets);

            var embed = GetCurrentLotteryEmbed.LotteryCommandEmbed(
                _discordClient,
                lotteryResponse,
                settings,
                account,
                userTickets,
                remainingHours,
                lastWinningTicket,
                lotteryResponse.TotalTickets,
                lotteryResponse.TotalParticipants);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("How many tickets would you like to purchase?")
                .AddEmbed(embed)
                .AddComponents(cancelBtn, helpBtn, buyXBtn, buyMaxTicketsBtn)
                .AsEphemeral(true));

            _discordClient.ComponentInteractionCreated += async (sender, args) =>
            {
                if (args.User.Id != userId)
                {
                    return;
                }

                if (args.Id == $"lottery_help_{uniqueId}")
                {
                    var helpEmbed = GetCurrentLotteryEmbed.LotteryHelpEmbed(_discordClient, lotteryResponse, settings);
                    await args.Interaction.CreateResponseAsync(
                        DiscordInteractionResponseType.UpdateMessage,
                        new DiscordInteractionResponseBuilder()
                        .ClearEmbeds()
                        .AddEmbed(helpEmbed));
                }

                if (args.Id == $"lottery_cancel_{uniqueId}")
                {
                    await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
                    await args.Interaction.DeleteOriginalResponseAsync();
                }

                if (args.Id == $"lottery_max_tickets_{uniqueId}")
                {
                    await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);

                    try
                    {
                        var buyCommand = new BuyTicketsCommand(args.User.Id, settings.MaxTickets);
                        var buyResponse = await _buyTicketsFacade.Execute(buyCommand, CancellationToken.None);

                        var updatedUserTickets = await _buyTickets.GetUserTicketsAsync(args.User.Id, lotteryResponse.Id, CancellationToken.None);
                        updatedUserTickets ??= new List<Fitz.Database.Entities.TicketEntity>();

                        int updatedRemainingHours = (int)(lotteryResponse.EndDate - DateTime.UtcNow).TotalHours;
                        int updatedLastWinningTicket = await _getCurrentLottery.GetLastWinningTicketAsync(CancellationToken.None);

                        var updatedLotteryResponse = await _getCurrentLotteryFacade.Execute(lotteryCommand, CancellationToken.None);

                        var updatedEmbed = GetCurrentLotteryEmbed.LotteryCommandEmbed(
                            _discordClient,
                            updatedLotteryResponse,
                            settings,
                            account,
                            updatedUserTickets,
                            updatedRemainingHours,
                            updatedLastWinningTicket,
                            updatedLotteryResponse.TotalTickets,
                            updatedLotteryResponse.TotalParticipants);

                        bool updatedUserHasMaxTickets = updatedUserTickets.Count >= settings.MaxTickets;
                        var updatedBuyMaxTicketsBtn = LotteryButtons.CreateBuyMaxTicketsButton(uniqueId, updatedUserHasMaxTickets);
                        var updatedBuyXBtn = LotteryButtons.CreateBuyXButton(uniqueId, updatedUserHasMaxTickets);

                        await args.Interaction.EditOriginalResponseAsync(
                            new DiscordWebhookBuilder()
                            .WithContent("How many tickets would you like to purchase?")
                            .AddEmbed(updatedEmbed)
                            .AddComponents(cancelBtn, helpBtn, updatedBuyXBtn, updatedBuyMaxTicketsBtn));
                    }
                    catch (AccountNotFound ex)
                    {
                        await args.Interaction.EditOriginalResponseAsync(
                            new DiscordWebhookBuilder()
                            .WithContent("You need to run `/signup` before you can interact with the lottery."));
                    }
                    catch (LotteryNotFound ex)
                    {
                        await args.Interaction.EditOriginalResponseAsync(
                            new DiscordWebhookBuilder()
                            .WithContent(ex.Message));
                    }
                    catch (InsufficientBeerException ex)
                    {
                        await args.Interaction.EditOriginalResponseAsync(
                            new DiscordWebhookBuilder()
                            .WithContent(ex.Message));
                    }
                    catch (MaxTicketsReachedException ex)
                    {
                        await args.Interaction.EditOriginalResponseAsync(
                            new DiscordWebhookBuilder()
                            .WithContent(ex.Message));
                    }
                    catch (InvalidTicketAmountException ex)
                    {
                        await args.Interaction.EditOriginalResponseAsync(
                            new DiscordWebhookBuilder()
                            .WithContent(ex.Message));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Buy max tickets failed - unexpected error. UserId: {UserId}", args.User.Id);
                        await args.Interaction.EditOriginalResponseAsync(
                            new DiscordWebhookBuilder()
                            .WithContent("An error occurred while purchasing tickets. Please try again later."));
                    }
                }

                if (args.Id == $"lottery_buy_x_{uniqueId}")
                {
                    try
                    {
                        var modal = ModalBuilder.Create("buy_x_tickets")
                            .WithTitle("Buy X Tickets")
                            .AddComponents(new DiscordTextInputComponent(
                                "How many tickets would you like to purchase?",
                                "tickets",
                                "Number of tickets",
                                required: true,
                                style: DiscordTextInputStyle.Short,
                                min_length: 1,
                                max_length: 3));

                        await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, modal);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create modal. UserId: {UserId}", args.User.Id);
                        await args.Interaction.CreateResponseAsync(
                            DiscordInteractionResponseType.ChannelMessageWithSource,
                            new DiscordInteractionResponseBuilder()
                            .WithContent("An error occurred while opening the ticket purchase form. Please try again later.")
                            .AsEphemeral(true));
                    }
                }
            };

            _logger.LogInformation("Lottery command completed successfully via Discord slash command. UserId: {UserId}, Username: {Username}", userId, username);
        }
        catch (LotteryNotFound ex)
        {
            _logger.LogWarning("Lottery command failed - lottery not found. UserId: {UserId}", userId);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent(ex.Message)
                .AsEphemeral(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lottery command failed - unexpected error. UserId: {UserId}, Username: {Username}", userId, username);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("An error occurred while retrieving lottery information. Please try again later.")
                .AsEphemeral(true));
        }
    }
}
