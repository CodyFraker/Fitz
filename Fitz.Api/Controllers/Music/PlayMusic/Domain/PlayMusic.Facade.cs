namespace Fitz.Api.Controllers.Music.PlayMusic.Domain;

public class PlayMusicFacade(PlayMusicService playMusicService, ILogger<PlayMusicFacade> logger)
{
    private readonly PlayMusicService _playMusicService = playMusicService;
    private readonly ILogger<PlayMusicFacade> _logger = logger;

    public async Task<PlayMusicModel> Execute(PlayMusicCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("PlayMusicFacade execution started. UserId: {UserId}, Song: {Song}", command.UserId, command.Song);

        var model = await _playMusicService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("PlayMusicFacade execution completed successfully. UserId: {UserId}, Song: {Song}, Success: {Success}", 
            command.UserId, command.Song, model.Success);

        return model;
    }
}
