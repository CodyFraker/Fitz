using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Settings.GetSettings.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Settings.GetSettings.Http;

[ApiController]
[Route("api/settings")]
public class GetSettingsController(GetSettingsFacade getSettingsFacade, ILogger<GetSettingsController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly GetSettingsFacade _getSettingsFacade = getSettingsFacade;
    private readonly ILogger<GetSettingsController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpGet]
    [RequireDiscordAuth]
    public async Task<IActionResult> GetSettings(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/settings";

        _fitzMetrics?.RecordApiRequest(endpoint, "GET");

        try
        {
            _logger.LogInformation("Get settings request received");

            var command = GetSettingsCommand.From();

            var response = await _getSettingsFacade.Execute(command, cancellationToken);

            var dto = GetSettingsResponseDto.From(response);

            _logger.LogInformation("Get settings completed successfully. SettingsId: {SettingsId}", response.Settings.Id);

            return Ok(new ApiResponse<GetSettingsResponseDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError("Get settings failed - invalid operation. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "internal_server_error");

            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get settings failed - unexpected error");
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
