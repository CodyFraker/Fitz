using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Admin.AdminUpdateFavorabilitySettings.Domain;

public interface IAdminUpdateFavorabilitySettings
{
    Task<SettingsEntity?> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<SettingsEntity> UpdateSettingsAsync(SettingsEntity settings, CancellationToken cancellationToken = default);
}
