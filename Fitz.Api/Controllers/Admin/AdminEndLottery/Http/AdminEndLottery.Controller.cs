using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Admin.AdminEndLottery.Domain;
using Fitz.Api.Controllers.Lottery.Exceptions;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Admin.AdminEndLottery.Http;

[ApiController]
[Route("api/admin/lottery")]
public class AdminEndLotteryController(AdminEndLotteryFacade adminEndLotteryFacade, ILogger<AdminEndLotteryController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly AdminEndLotteryFacade _adminEndLotteryFacade = adminEndLotteryFacade;
    private readonly ILogger<AdminEndLotteryController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPost("current/end")]
    [RequireDiscordAuth]
    [RequireAdmin]
    public async Task<IActionResult> EndLottery(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/admin/lottery/current/end";

        _fitzMetrics?.RecordApiRequest(endpoint, "POST");

        try
        {
            _logger.LogInformation("Admin end lottery request received");

            var command = AdminEndLotteryCommand.From();

            var response = await _adminEndLotteryFacade.Execute(command, cancellationToken);

            _logger.LogInformation("Admin end lottery completed successfully. Message: {Message}", response.Message);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = response.Message
            });
        }
        catch (LotteryNotFound ex)
        {
            _logger.LogWarning("Admin end lottery failed - lottery not found");
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin end lottery failed - unexpected error");
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
