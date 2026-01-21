namespace Fitz.Api.Controllers.Admin.AdminUpdateFavorabilitySettings.Domain;

public record AdminUpdateFavorabilitySettingsResponse(
    string Message)
{
    public static AdminUpdateFavorabilitySettingsResponse From(AdminUpdateFavorabilitySettingsModel model)
    {
        return new AdminUpdateFavorabilitySettingsResponse(Message: model.Message);
    }
}
