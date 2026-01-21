namespace Fitz.Api.Controllers.Admin.AdminUpdateFavorabilitySettings.Domain;

public record AdminUpdateFavorabilitySettingsModel(
    string Message)
{
    public static AdminUpdateFavorabilitySettingsModel From(string message)
    {
        return new AdminUpdateFavorabilitySettingsModel(Message: message);
    }
}
