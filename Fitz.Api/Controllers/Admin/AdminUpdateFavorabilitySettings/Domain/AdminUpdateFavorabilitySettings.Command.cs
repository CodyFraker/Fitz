using Fitz.Api.Controllers.Admin.AdminUpdateFavorabilitySettings.Http;

namespace Fitz.Api.Controllers.Admin.AdminUpdateFavorabilitySettings.Domain;

public record AdminUpdateFavorabilitySettingsCommand(
    decimal? FavorabilityBeerRatioThreshold,
    int? FavorabilityLowThreshold,
    decimal? FavorabilityBaseDropPercent,
    decimal? FavorabilityDropMultiplier)
{
    public static AdminUpdateFavorabilitySettingsCommand From(UpdateFavorabilitySettingsRequestDto request)
    {
        return new AdminUpdateFavorabilitySettingsCommand(
            FavorabilityBeerRatioThreshold: request.FavorabilityBeerRatioThreshold,
            FavorabilityLowThreshold: request.FavorabilityLowThreshold,
            FavorabilityBaseDropPercent: request.FavorabilityBaseDropPercent,
            FavorabilityDropMultiplier: request.FavorabilityDropMultiplier
        );
    }
}
