using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Admin.AdminCancelLottery.Domain;
using Fitz.Api.Controllers.Lottery.Exceptions;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Admin.AdminCancelLottery.Http;

[ApiController]
[Route("api/admin/lottery")]
public class AdminCancelLotteryController(AdminCancelLotteryFacade adminCancelLotteryFacade, ILogger<AdminCancelLotteryController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly AdminCancelLotteryFacade _adminCancelLotteryFacade = adminCancelLotteryFacade;
    private readonly ILogger<AdminCancelLotteryController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpDelete("current")]
    [RequireDiscordAuth]
    [RequireAdmin]
    public async Task<IActionResult> CancelCurrentLottery(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/admin/lottery/current";

        _fitzMetrics?.RecordApiRequest(endpoint, "DELETE");

        try
        {
            _logger.LogInformation("Admin cancel lottery request received");

            var command = AdminCancelLotteryCommand.From();

            var response = await _adminCancelLotteryFacade.Execute(command, cancellationToken);

            _logger.LogInformation("Admin cancel lottery completed successfully. Message: {Message}", response.Message);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = response.Message
            });
        }
        catch (LotteryNotFound ex)
        {
            _logger.LogWarning("Admin cancel lottery failed - lottery not found");
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin cancel lottery failed - unexpected error");
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
