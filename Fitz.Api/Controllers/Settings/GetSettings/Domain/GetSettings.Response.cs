using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Settings.GetSettings.Domain;

public record GetSettingsResponse(
    SettingsEntity Settings)
{
    public static GetSettingsResponse From(GetSettingsModel model)
    {
        return new GetSettingsResponse(Settings: model.Settings);
    }
}
