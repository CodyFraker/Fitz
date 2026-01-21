namespace Fitz.Api.Controllers.Admin.AdminUpdateFavorabilitySettings.Domain;

public class AdminUpdateFavorabilitySettingsService(
    IAdminUpdateFavorabilitySettings adminUpdateFavorabilitySettings,
    ILogger<AdminUpdateFavorabilitySettingsService> logger)
{
    private readonly IAdminUpdateFavorabilitySettings _adminUpdateFavorabilitySettings = adminUpdateFavorabilitySettings;
    private readonly ILogger<AdminUpdateFavorabilitySettingsService> _logger = logger;

    public async Task<AdminUpdateFavorabilitySettingsModel> ExecuteAsync(AdminUpdateFavorabilitySettingsCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AdminUpdateFavorabilitySettingsService execution started");

        var settings = await _adminUpdateFavorabilitySettings.GetSettingsAsync(cancellationToken);
        if (settings == null)
        {
            _logger.LogError("Settings not found");
            throw new InvalidOperationException("Settings not found");
        }

        bool hasChanges = false;

        if (command.FavorabilityBeerRatioThreshold.HasValue)
        {
            if (command.FavorabilityBeerRatioThreshold.Value < 0.1m || command.FavorabilityBeerRatioThreshold.Value > 100)
            {
                _logger.LogError("AdminUpdateFavorabilitySettings validation failed - FavorabilityBeerRatioThreshold must be between 0.1 and 100. Value: {Value}", 
                    command.FavorabilityBeerRatioThreshold.Value);
                throw new ArgumentException("FavorabilityBeerRatioThreshold must be between 0.1 and 100.", nameof(command.FavorabilityBeerRatioThreshold));
            }
            settings.FavorabilityBeerRatioThreshold = command.FavorabilityBeerRatioThreshold.Value;
            hasChanges = true;
        }

        if (command.FavorabilityLowThreshold.HasValue)
        {
            if (command.FavorabilityLowThreshold.Value < 0 || command.FavorabilityLowThreshold.Value > 100)
            {
                _logger.LogError("AdminUpdateFavorabilitySettings validation failed - FavorabilityLowThreshold must be between 0 and 100. Value: {Value}", 
                    command.FavorabilityLowThreshold.Value);
                throw new ArgumentException("FavorabilityLowThreshold must be between 0 and 100.", nameof(command.FavorabilityLowThreshold));
            }
            settings.FavorabilityLowThreshold = command.FavorabilityLowThreshold.Value;
            hasChanges = true;
        }

        if (command.FavorabilityBaseDropPercent.HasValue)
        {
            if (command.FavorabilityBaseDropPercent.Value < 0 || command.FavorabilityBaseDropPercent.Value > 100)
            {
                _logger.LogError("AdminUpdateFavorabilitySettings validation failed - FavorabilityBaseDropPercent must be between 0 and 100. Value: {Value}", 
                    command.FavorabilityBaseDropPercent.Value);
                throw new ArgumentException("FavorabilityBaseDropPercent must be between 0 and 100.", nameof(command.FavorabilityBaseDropPercent));
            }
            settings.FavorabilityBaseDropPercent = command.FavorabilityBaseDropPercent.Value;
            hasChanges = true;
        }

        if (command.FavorabilityDropMultiplier.HasValue)
        {
            if (command.FavorabilityDropMultiplier.Value < 0.1m || command.FavorabilityDropMultiplier.Value > 10)
            {
                _logger.LogError("AdminUpdateFavorabilitySettings validation failed - FavorabilityDropMultiplier must be between 0.1 and 10. Value: {Value}", 
                    command.FavorabilityDropMultiplier.Value);
                throw new ArgumentException("FavorabilityDropMultiplier must be between 0.1 and 10.", nameof(command.FavorabilityDropMultiplier));
            }
            settings.FavorabilityDropMultiplier = command.FavorabilityDropMultiplier.Value;
            hasChanges = true;
        }

        if (!hasChanges)
        {
            _logger.LogWarning("No changes to apply to favorability settings");
            throw new ArgumentException("At least one setting must be provided to update.");
        }

        await _adminUpdateFavorabilitySettings.UpdateSettingsAsync(settings, cancellationToken);

        var model = AdminUpdateFavorabilitySettingsModel.From("Favorability settings updated successfully");

        _logger.LogInformation("AdminUpdateFavorabilitySettingsModel created successfully");

        return model;
    }
}
