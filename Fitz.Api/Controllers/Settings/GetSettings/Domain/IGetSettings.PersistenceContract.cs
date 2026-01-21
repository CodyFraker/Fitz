using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Settings.GetSettings.Domain;

public interface IGetSettings
{
    Task<SettingsEntity> GetSettingsAsync(CancellationToken cancellationToken = default);
}
