using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Bank.GetBalanceWithTransactions.Domain;
using Fitz.Api.Controllers.Bank.GetBalanceWithTransactions.Embeds;

namespace Fitz.Api.Controllers.Bank.GetBalanceWithTransactions.Discord;

[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public sealed class GetBalanceWithTransactionsSlashCommand(
    GetBalanceWithTransactionsFacade getBalanceWithTransactionsFacade,
    DiscordClient discordClient,
    ILogger<GetBalanceWithTransactionsSlashCommand> logger) : ApplicationCommandModule
{
    private readonly GetBalanceWithTransactionsFacade _getBalanceWithTransactionsFacade = getBalanceWithTransactionsFacade;
    private readonly DiscordClient _discordClient = discordClient;
    private readonly ILogger<GetBalanceWithTransactionsSlashCommand> _logger = logger;

    [SlashCommand("fridge", "Check how much beer you have in the fridge.")]
    public async Task Fridge(InteractionContext ctx)
    {
        var userId = ctx.User.Id;
        var username = ctx.User.Username;

        _logger.LogInformation("Fridge command started via Discord slash command. UserId: {UserId}, Username: {Username}", userId, username);

        try
        {
            var command = GetBalanceWithTransactionsCommand.From(userId);

            var model = await _getBalanceWithTransactionsFacade.Execute(command, CancellationToken.None);

            var embed = BalanceEmbed.FromGetBalanceWithTransactions(_discordClient, model);

            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(embed)
                .AsEphemeral(true));

            _logger.LogInformation("Fridge command completed successfully via Discord slash command. UserId: {UserId}, Username: {Username}, Beer: {Beer}", userId, username, model.Account.Beer);
        }
        catch (AccountNotFound ex)
        {
            _logger.LogWarning("Fridge command failed - account not found. UserId: {UserId}", ex.UserId);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("You do not have an account. Please sign up using `/signup`.")
                .AsEphemeral(true));
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Fridge command failed - invalid argument. UserId: {UserId}, Username: {Username}, Error: {Error}", userId, username, ex.Message);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent($"Invalid request: {ex.Message}")
                .AsEphemeral(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fridge command failed - unexpected error. UserId: {UserId}, Username: {Username}", userId, username);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("An error occurred while retrieving your balance. Please try again later.")
                .AsEphemeral(true));
        }
    }
}
