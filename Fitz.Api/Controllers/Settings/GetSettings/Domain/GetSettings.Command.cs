namespace Fitz.Api.Controllers.Settings.GetSettings.Domain;

public record GetSettingsCommand
{
    public static GetSettingsCommand From()
    {
        return new GetSettingsCommand();
    }
}
