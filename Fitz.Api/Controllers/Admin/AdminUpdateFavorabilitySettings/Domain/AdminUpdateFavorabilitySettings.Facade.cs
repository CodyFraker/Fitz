namespace Fitz.Api.Controllers.Admin.AdminUpdateFavorabilitySettings.Domain;

public class AdminUpdateFavorabilitySettingsFacade(AdminUpdateFavorabilitySettingsService adminUpdateFavorabilitySettingsService, ILogger<AdminUpdateFavorabilitySettingsFacade> logger)
{
    private readonly AdminUpdateFavorabilitySettingsService _adminUpdateFavorabilitySettingsService = adminUpdateFavorabilitySettingsService;
    private readonly ILogger<AdminUpdateFavorabilitySettingsFacade> _logger = logger;

    public async Task<AdminUpdateFavorabilitySettingsResponse> Execute(AdminUpdateFavorabilitySettingsCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AdminUpdateFavorabilitySettingsFacade execution started");

        var model = await _adminUpdateFavorabilitySettingsService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("AdminUpdateFavorabilitySettingsService execution completed. Message: {Message}", model.Message);

        var response = AdminUpdateFavorabilitySettingsResponse.From(model);

        _logger.LogInformation("AdminUpdateFavorabilitySettingsFacade execution completed successfully. Message: {Message}", model.Message);

        return response;
    }
}
