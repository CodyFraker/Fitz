using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Settings.GetSettings.Domain;

public record GetSettingsModel(
    SettingsEntity Settings)
{
    public static GetSettingsModel From(SettingsEntity settings)
    {
        return new GetSettingsModel(Settings: settings);
    }
}
