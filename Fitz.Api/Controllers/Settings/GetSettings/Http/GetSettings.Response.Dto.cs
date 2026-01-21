using Fitz.Api.Controllers.Settings.GetSettings.Domain;
using Fitz.Api.Models.Responses;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Settings.GetSettings.Http;

[DisplayName("GetSettingsResponse")]
public record GetSettingsResponseDto
{
    [Required]
    public required SettingsResponse Settings { get; set; }

    public static GetSettingsResponseDto From(GetSettingsResponse response)
    {
        return new GetSettingsResponseDto
        {
            Settings = new SettingsResponse
            {
                PollSubmittedPenalty = response.Settings.PollSubmittedPenalty,
                PollDeclinedPenalty = response.Settings.PollDeclinedPenalty,
                MaxPendingPolls = response.Settings.MaxPendingPolls,
                FavorabilityBeerRatioThreshold = response.Settings.FavorabilityBeerRatioThreshold,
                FavorabilityLowThreshold = response.Settings.FavorabilityLowThreshold,
                FavorabilityBaseDropPercent = response.Settings.FavorabilityBaseDropPercent,
                FavorabilityDropMultiplier = response.Settings.FavorabilityDropMultiplier
            }
        };
    }
}
