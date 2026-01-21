namespace Fitz.Api.Controllers.Settings.GetSettings.Domain;

public class GetSettingsService(IGetSettings getSettings, ILogger<GetSettingsService> logger)
{
    private readonly IGetSettings _getSettings = getSettings;
    private readonly ILogger<GetSettingsService> _logger = logger;

    public async Task<GetSettingsModel> ExecuteAsync(GetSettingsCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetSettingsService execution started");

        var settings = await _getSettings.GetSettingsAsync(cancellationToken);

        if (settings == null)
        {
            _logger.LogError("Settings not found and could not be created");
            throw new InvalidOperationException("Settings not found and could not be created");
        }

        var model = GetSettingsModel.From(settings);

        _logger.LogInformation("GetSettingsModel created successfully. SettingsId: {SettingsId}", settings.Id);

        return model;
    }
}
