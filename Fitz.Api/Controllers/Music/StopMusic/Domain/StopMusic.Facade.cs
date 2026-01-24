namespace Fitz.Api.Controllers.Music.StopMusic.Domain;

public class StopMusicFacade(StopMusicService stopMusicService, ILogger<StopMusicFacade> logger)
{
    private readonly StopMusicService _stopMusicService = stopMusicService;
    private readonly ILogger<StopMusicFacade> _logger = logger;

    public async Task<StopMusicModel> Execute(StopMusicCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("StopMusicFacade execution started. UserId: {UserId}", command.UserId);

        var model = await _stopMusicService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("StopMusicFacade execution completed successfully. UserId: {UserId}, Success: {Success}", 
            command.UserId, model.Success);

        return model;
    }
}
