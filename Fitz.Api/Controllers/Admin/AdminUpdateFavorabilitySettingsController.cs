using Fitz.Api.Attributes;
using Fitz.Api.Models.Requests;
using Fitz.Api.Models.Responses;
using Fitz.Database;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/favorability")]
    public class AdminUpdateFavorabilitySettingsController : ControllerBase
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FitzMetrics? _fitzMetrics;

        public AdminUpdateFavorabilitySettingsController(IServiceScopeFactory scopeFactory, FitzMetrics? fitzMetrics = null)
        {
            _scopeFactory = scopeFactory;
            _fitzMetrics = fitzMetrics;
        }

        [HttpPatch("settings")]
        [RequireDiscordAuth]
        [RequireAdmin]
        public async Task<IActionResult> UpdateFavorabilitySettings([FromBody] UpdateFavorabilitySettingsRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/admin/favorability/settings";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "PATCH");
            
            try
            {
                if (!ModelState.IsValid)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "validation_error");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request"
                    });
                }

                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                var settings = db.Settings.FirstOrDefault();
                if (settings == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Settings not found"
                    });
                }

                if (request.FavorabilityBeerRatioThreshold.HasValue)
                {
                    settings.FavorabilityBeerRatioThreshold = request.FavorabilityBeerRatioThreshold.Value;
                }

                if (request.FavorabilityLowThreshold.HasValue)
                {
                    settings.FavorabilityLowThreshold = request.FavorabilityLowThreshold.Value;
                }

                if (request.FavorabilityBaseDropPercent.HasValue)
                {
                    settings.FavorabilityBaseDropPercent = request.FavorabilityBaseDropPercent.Value;
                }

                if (request.FavorabilityDropMultiplier.HasValue)
                {
                    settings.FavorabilityDropMultiplier = request.FavorabilityDropMultiplier.Value;
                }

                db.Update(settings);
                await db.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Favorability settings updated successfully"
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
