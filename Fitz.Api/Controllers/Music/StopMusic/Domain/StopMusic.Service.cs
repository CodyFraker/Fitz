using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Microsoft.Extensions.Options;

namespace Fitz.Api.Controllers.Music.StopMusic.Domain;

public class StopMusicService(
    IAudioService audioService,
    ILogger<StopMusicService> logger)
{
    private readonly IAudioService _audioService = audioService;
    private readonly ILogger<StopMusicService> _logger = logger;

    public async Task<StopMusicModel> ExecuteAsync(StopMusicCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("StopMusicService execution started. UserId: {UserId}, GuildId: {GuildId}", command.UserId, command.GuildId);

        var channelBehavior = PlayerChannelBehavior.None;
        var retrieveOptions = new PlayerRetrieveOptions(ChannelBehavior: channelBehavior);

        var options = new QueuedLavalinkPlayerOptions
        {
            SelfDeaf = true,
            SelfMute = false,
            DisconnectOnStop = false,
        };

        var optionsWrapper = Options.Create(options);

        var result = await _audioService.Players
            .RetrieveAsync(command.GuildId, command.VoiceChannelId, PlayerFactory.Queued, optionsWrapper, retrieveOptions).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            var errorMessage = result.Status switch
            {
                PlayerRetrieveStatus.UserNotInVoiceChannel => "You are not connected to a voice channel.",
                PlayerRetrieveStatus.BotNotConnected => "The bot is currently not connected.",
                _ => "Unknown error.",
            };

            _logger.LogWarning("Failed to retrieve player. UserId: {UserId}, Status: {Status}", command.UserId, result.Status);
            return StopMusicModel.From(command.UserId, false, errorMessage);
        }

        var player = result.Player;

        if (player.CurrentTrack == null)
        {
            _logger.LogWarning("No track currently playing. UserId: {UserId}", command.UserId);
            return StopMusicModel.From(command.UserId, false, "There is nothing playing.");
        }

        await player.StopAsync().ConfigureAwait(false);

        _logger.LogInformation("StopMusicModel created successfully. UserId: {UserId}", command.UserId);

        return StopMusicModel.From(command.UserId, true, "Stopping..");
    }
}
