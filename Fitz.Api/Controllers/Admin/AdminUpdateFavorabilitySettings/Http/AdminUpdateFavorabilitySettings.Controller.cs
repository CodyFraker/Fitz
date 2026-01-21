using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Admin.AdminUpdateFavorabilitySettings.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Admin.AdminUpdateFavorabilitySettings.Http;

[ApiController]
[Route("api/admin/favorability")]
public class AdminUpdateFavorabilitySettingsController(AdminUpdateFavorabilitySettingsFacade adminUpdateFavorabilitySettingsFacade, ILogger<AdminUpdateFavorabilitySettingsController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly AdminUpdateFavorabilitySettingsFacade _adminUpdateFavorabilitySettingsFacade = adminUpdateFavorabilitySettingsFacade;
    private readonly ILogger<AdminUpdateFavorabilitySettingsController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPatch("settings")]
    [RequireDiscordAuth]
    [RequireAdmin]
    public async Task<IActionResult> UpdateFavorabilitySettings([FromBody] UpdateFavorabilitySettingsRequestDto request, CancellationToken cancellationToken = default)
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

            _logger.LogInformation("Admin update favorability settings request received");

            var command = request.ToCommand();

            var response = await _adminUpdateFavorabilitySettingsFacade.Execute(command, cancellationToken);

            _logger.LogInformation("Admin update favorability settings completed successfully. Message: {Message}", response.Message);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = response.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Admin update favorability settings failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "validation_error");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError("Admin update favorability settings failed - invalid operation. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin update favorability settings failed - unexpected error");
            _fitzMetrics?.RecordApiError(endpoint, "internal_server_error");

            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        finally
        {
            stopwatch.Stop();
            _fitzMetrics?.RecordApiRequestDuration(endpoint, stopwatch.Elapsed.TotalSeconds);
        }
    }
}
