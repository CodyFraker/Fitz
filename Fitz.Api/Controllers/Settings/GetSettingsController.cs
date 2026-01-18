using Fitz.Api.Attributes;
using Fitz.Api.Models.Responses;
using Fitz.Features.Settings;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Settings
{
    [ApiController]
    [Route("api/settings")]
    public class GetSettingsController : ControllerBase
    {
        private readonly SettingsService _settingsService;
        private readonly FitzMetrics? _fitzMetrics;

        public GetSettingsController(SettingsService settingsService, FitzMetrics? fitzMetrics = null)
        {
            _settingsService = settingsService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpGet]
        [RequireDiscordAuth]
        public IActionResult GetSettings()
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/settings";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                var settings = _settingsService.GetSettings();

                var response = new SettingsResponse
                {
                    PollSubmittedPenalty = settings.PollSubmittedPenalty,
                    PollDeclinedPenalty = settings.PollDeclinedPenalty,
                    MaxPendingPolls = settings.MaxPendingPolls,
                    FavorabilityBeerRatioThreshold = settings.FavorabilityBeerRatioThreshold,
                    FavorabilityLowThreshold = settings.FavorabilityLowThreshold,
                    FavorabilityBaseDropPercent = settings.FavorabilityBaseDropPercent,
                    FavorabilityDropMultiplier = settings.FavorabilityDropMultiplier
                };

                return Ok(new ApiResponse<SettingsResponse>
                {
                    Success = true,
                    Data = response
                });
            }
            finally
            {
                stopwatch.Stop();
                _fitzMetrics?.RecordApiRequestDuration(endpoint, stopwatch.Elapsed.TotalSeconds);
            }
        }
    }
}
