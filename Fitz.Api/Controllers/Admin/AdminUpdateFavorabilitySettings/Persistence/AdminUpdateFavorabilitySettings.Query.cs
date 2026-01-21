using Fitz.Api.Controllers.Admin.AdminUpdateFavorabilitySettings.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Admin.AdminUpdateFavorabilitySettings.Persistence;

public class AdminUpdateFavorabilitySettings(IDbContextFactory<BotContext> contextFactory, ILogger<AdminUpdateFavorabilitySettings> logger) : IAdminUpdateFavorabilitySettings
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<AdminUpdateFavorabilitySettings> _logger = logger;

    public async Task<SettingsEntity?> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting settings");

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var settings = await context.Settings.FirstOrDefaultAsync(cancellationToken);

        if (settings != null)
        {
            _logger.LogInformation("Settings found. SettingsId: {SettingsId}", settings.Id);
        }
        else
        {
            _logger.LogWarning("Settings not found");
        }

        return settings;
    }

    public async Task<SettingsEntity> UpdateSettingsAsync(SettingsEntity settings, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating settings. SettingsId: {SettingsId}", settings.Id);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.Settings.Update(settings);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Settings updated successfully. SettingsId: {SettingsId}", settings.Id);

        return settings;
    }
}
