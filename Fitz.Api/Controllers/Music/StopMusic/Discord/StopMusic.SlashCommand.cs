using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Api.Controllers.Music.StopMusic.Domain;

namespace Fitz.Api.Controllers.Music.StopMusic.Discord;

[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public sealed class StopMusicSlashCommand(StopMusicFacade stopMusicFacade, ILogger<StopMusicSlashCommand> logger) : ApplicationCommandModule
{
    private readonly StopMusicFacade _stopMusicFacade = stopMusicFacade;
    private readonly ILogger<StopMusicSlashCommand> _logger = logger;

    [SlashCommand("stop", "Stop the current song.")]
    public async Task Stop(InteractionContext ctx)
    {
        var userId = ctx.User.Id;
        var username = ctx.User.Username;

        _logger.LogInformation("Stop music command started via Discord slash command. UserId: {UserId}, Username: {Username}", userId, username);

        try
        {
            var command = StopMusicCommand.FromInteractionContext(ctx);

            var model = await _stopMusicFacade.Execute(command, CancellationToken.None);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent(model.Message ?? (model.Success ? "Stopping.." : "Failed to stop."))
                .AsEphemeral(true));

            _logger.LogInformation("Stop music command completed via Discord slash command. UserId: {UserId}, Username: {Username}, Success: {Success}", userId, username, model.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stop music command failed - unexpected error. UserId: {UserId}, Username: {Username}", userId, username);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("An error occurred while stopping the song. Please try again later.")
                .AsEphemeral(true));
        }
    }
}
