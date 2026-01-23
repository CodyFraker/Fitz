using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Lottery.BuyTickets.Domain;
using Fitz.Api.Controllers.Lottery.Embeds;
using Fitz.Api.Controllers.Lottery.Exceptions;

namespace Fitz.Api.Controllers.Lottery.BuyTickets.Discord;

[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public sealed class BuyTicketsSlashCommand(BuyTicketsFacade buyTicketsFacade, ILogger<BuyTicketsSlashCommand> logger) : ApplicationCommandModule
{
    private readonly BuyTicketsFacade _buyTicketsFacade = buyTicketsFacade;
    private readonly ILogger<BuyTicketsSlashCommand> _logger = logger;

    [SlashCommand("buyTickets", "Play stupid games. Win beer. Lose beer.")]
    public async Task BuyTickets(InteractionContext ctx, [Option("Tickets", "How many tickets do you want?")] long tickets)
    {
        var userId = ctx.User.Id;
        var username = ctx.User.Username;

        _logger.LogInformation("Buy tickets started via Discord slash command. UserId: {UserId}, Username: {Username}, Tickets: {Tickets}", userId, username, tickets);

        try
        {
            var command = BuyTicketsCommand.FromInteractionContext(ctx, (int)tickets);

            var response = await _buyTicketsFacade.Execute(command, CancellationToken.None);

            var buyTicketsEmbed = BuyTicketsEmbed.FromBuyTickets(response);

            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(buyTicketsEmbed)
                .AsEphemeral(true));

            _logger.LogInformation("Buy tickets completed successfully via Discord slash command. UserId: {UserId}, Username: {Username}, TicketsPurchased: {TicketsPurchased}", userId, username, response.TicketsPurchased);
        }
        catch (AccountNotFound ex)
        {
            _logger.LogWarning("Buy tickets failed - account not found. UserId: {UserId}", ex.UserId);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("You need to run `/signup` before you can interact with the lottery.")
                .AsEphemeral(true));
        }
        catch (LotteryNotFound ex)
        {
            _logger.LogWarning("Buy tickets failed - lottery not found. UserId: {UserId}", userId);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent(ex.Message)
                .AsEphemeral(true));
        }
        catch (InsufficientBeerException ex)
        {
            _logger.LogWarning("Buy tickets failed - insufficient beer. UserId: {UserId}, Required: {Required}, Current: {Current}", userId, ex.RequiredAmount, ex.CurrentBalance);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent(ex.Message)
                .AsEphemeral(true));
        }
        catch (MaxTicketsReachedException ex)
        {
            _logger.LogWarning("Buy tickets failed - max tickets reached. UserId: {UserId}, Current: {Current}, Max: {Max}", userId, ex.CurrentTicketCount, ex.MaxTickets);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent(ex.Message)
                .AsEphemeral(true));
        }
        catch (InvalidTicketAmountException ex)
        {
            _logger.LogWarning("Buy tickets failed - invalid ticket amount. UserId: {UserId}, Amount: {Amount}, Reason: {Reason}", userId, ex.RequestedAmount, ex.Reason);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent(ex.Message)
                .AsEphemeral(true));
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Buy tickets failed - invalid argument. UserId: {UserId}, Username: {Username}, Error: {Error}", userId, username, ex.Message);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent($"Invalid request: {ex.Message}")
                .AsEphemeral(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Buy tickets failed - unexpected error. UserId: {UserId}, Username: {Username}", userId, username);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("An error occurred while purchasing tickets. Please try again later.")
                .AsEphemeral(true));
        }
    }
}
