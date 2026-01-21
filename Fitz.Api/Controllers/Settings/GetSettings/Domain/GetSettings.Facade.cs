namespace Fitz.Api.Controllers.Settings.GetSettings.Domain;

public class GetSettingsFacade(GetSettingsService getSettingsService, ILogger<GetSettingsFacade> logger)
{
    private readonly GetSettingsService _getSettingsService = getSettingsService;
    private readonly ILogger<GetSettingsFacade> _logger = logger;

    public async Task<GetSettingsResponse> Execute(GetSettingsCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetSettingsFacade execution started");

        var model = await _getSettingsService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("GetSettingsService execution completed. SettingsId: {SettingsId}", model.Settings.Id);

        var response = GetSettingsResponse.From(model);

        _logger.LogInformation("GetSettingsFacade execution completed successfully. SettingsId: {SettingsId}", model.Settings.Id);

        return response;
    }
}
