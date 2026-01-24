using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Options;

namespace Fitz.Api.Controllers.Music.PlayMusic.Domain;

public class PlayMusicService(
    IAudioService audioService,
    ILogger<PlayMusicService> logger)
{
    private readonly IAudioService _audioService = audioService;
    private readonly ILogger<PlayMusicService> _logger = logger;

    public async Task<PlayMusicModel> ExecuteAsync(PlayMusicCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("PlayMusicService execution started. UserId: {UserId}, GuildId: {GuildId}, Song: {Song}", command.UserId, command.GuildId, command.Song);

        if (string.IsNullOrWhiteSpace(command.Song))
        {
            _logger.LogError("PlayMusic validation failed - Song cannot be empty.");
            throw new ArgumentException("Song cannot be empty.", nameof(command.Song));
        }

        if (command.VoiceChannelId == null)
        {
            _logger.LogWarning("User not in voice channel. UserId: {UserId}", command.UserId);
            return PlayMusicModel.From(command.UserId, command.Song, false, "You are not connected to a voice channel.");
        }

        var channelBehavior = PlayerChannelBehavior.Join;
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
            return PlayMusicModel.From(command.UserId, command.Song, false, errorMessage);
        }

        var player = result.Player;

        var loadOptions = new TrackLoadOptions
        {
            SearchMode = TrackSearchMode.YouTube,
        };

        var track = await _audioService.Tracks.LoadTrackAsync(command.Song, loadOptions).ConfigureAwait(false);

        if (track == null)
        {
            _logger.LogWarning("Track not found. UserId: {UserId}, Song: {Song}", command.UserId, command.Song);
            return PlayMusicModel.From(command.UserId, command.Song, false, "Track not found.");
        }

        await player.PlayAsync(command.Song).ConfigureAwait(false);

        _logger.LogInformation("PlayMusicModel created successfully. UserId: {UserId}, Song: {Song}", command.UserId, command.Song);

        return PlayMusicModel.From(command.UserId, command.Song, true, "Playing song...");
    }
}
