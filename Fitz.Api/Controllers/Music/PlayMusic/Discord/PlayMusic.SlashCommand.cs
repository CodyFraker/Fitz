using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Api.Controllers.Music.PlayMusic.Domain;
using Fitz.Core.Commands.Attributes;

namespace Fitz.Api.Controllers.Music.PlayMusic.Discord;

[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public sealed class PlayMusicSlashCommand(PlayMusicFacade playMusicFacade, ILogger<PlayMusicSlashCommand> logger) : ApplicationCommandModule
{
    private readonly PlayMusicFacade _playMusicFacade = playMusicFacade;
    private readonly ILogger<PlayMusicSlashCommand> _logger = logger;

    [SlashCommand("play", "Play a song at the cost of beer.")]
    [RequireAccount]
    public async Task Play(InteractionContext ctx, [Option("song", "The song to play.")] string song)
    {
        var userId = ctx.User.Id;
        var username = ctx.User.Username;

        _logger.LogInformation("Play music command started via Discord slash command. UserId: {UserId}, Username: {Username}, Song: {Song}", userId, username, song);

        try
        {
            var command = PlayMusicCommand.FromInteractionContext(ctx, song);

            var model = await _playMusicFacade.Execute(command, CancellationToken.None);

            if (model.Success)
            {
                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent(model.Message ?? "Playing song...")
                    .AsEphemeral(true));
            }
            else
            {
                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent(model.Message ?? "Failed to play song.")
                    .AsEphemeral(true));
            }

            _logger.LogInformation("Play music command completed via Discord slash command. UserId: {UserId}, Username: {Username}, Success: {Success}", userId, username, model.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Play music command failed - unexpected error. UserId: {UserId}, Username: {Username}", userId, username);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("An error occurred while playing the song. Please try again later.")
                .AsEphemeral(true));
        }
    }
}
