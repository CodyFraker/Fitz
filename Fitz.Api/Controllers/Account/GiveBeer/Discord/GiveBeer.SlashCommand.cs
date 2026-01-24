using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Account.GiveBeer.Domain;

namespace Fitz.Api.Controllers.Account.GiveBeer.Discord;

[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public sealed class GiveBeerSlashCommand(GiveBeerFacade giveBeerFacade, ILogger<GiveBeerSlashCommand> logger) : ApplicationCommandModule
{
    private readonly GiveBeerFacade _giveBeerFacade = giveBeerFacade;
    private readonly ILogger<GiveBeerSlashCommand> _logger = logger;

    [SlashCommand("beer", "Give a beer to Fitz")]
    public async Task GiveBeer(InteractionContext ctx, [Option("Beer", "How much beer do you want to give Fitz?", false)] double amount = 0)
    {
        var userId = ctx.User.Id;
        var username = ctx.User.Username;

        _logger.LogInformation("Give beer command started via Discord slash command. UserId: {UserId}, Username: {Username}, Amount: {Amount}", userId, username, amount);

        if (amount <= 0)
        {
            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("You need to specify an amount greater than 0.")
                .AsEphemeral(true));
            return;
        }

        try
        {
            var command = GiveBeerCommand.FromInteractionContext(ctx, amount);

            var model = await _giveBeerFacade.Execute(command, CancellationToken.None);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent(model.Message)
                .AsEphemeral(true));

            _logger.LogInformation("Give beer command completed successfully via Discord slash command. UserId: {UserId}, Username: {Username}, Amount: {Amount}", userId, username, amount);
        }
        catch (AccountNotFound ex)
        {
            _logger.LogWarning("Give beer failed - account not found. UserId: {UserId}", ex.UserId);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("You don't have an account yet!")
                .AsEphemeral(true));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Give beer failed - invalid operation. UserId: {UserId}, Error: {Error}", userId, ex.Message);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent(ex.Message)
                .AsEphemeral(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Give beer failed - unexpected error. UserId: {UserId}, Username: {Username}", userId, username);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("An error occurred while giving beer. Please try again later.")
                .AsEphemeral(true));
        }
    }
}
