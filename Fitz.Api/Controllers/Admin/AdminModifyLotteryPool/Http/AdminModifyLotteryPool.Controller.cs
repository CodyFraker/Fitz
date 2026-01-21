using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Admin.AdminModifyLotteryPool.Domain;
using Fitz.Api.Controllers.Lottery.Exceptions;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Admin.AdminModifyLotteryPool.Http;

[ApiController]
[Route("api/admin/lottery")]
public class AdminModifyLotteryPoolController(AdminModifyLotteryPoolFacade adminModifyLotteryPoolFacade, ILogger<AdminModifyLotteryPoolController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly AdminModifyLotteryPoolFacade _adminModifyLotteryPoolFacade = adminModifyLotteryPoolFacade;
    private readonly ILogger<AdminModifyLotteryPoolController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPatch("current/pool")]
    [RequireDiscordAuth]
    [RequireAdmin]
    public async Task<IActionResult> ModifyLotteryPool([FromBody] AdminModifyLotteryPoolRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/admin/lottery/current/pool";

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

            _logger.LogInformation("Admin modify lottery pool request received. Pool: {Pool}", request.Pool);

            var command = request.ToCommand();

            var response = await _adminModifyLotteryPoolFacade.Execute(command, cancellationToken);

            _logger.LogInformation("Admin modify lottery pool completed successfully. Message: {Message}", response.Message);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = response.Message
            });
        }
        catch (LotteryNotFound ex)
        {
            _logger.LogWarning("Admin modify lottery pool failed - lottery not found");
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Admin modify lottery pool failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "validation_error");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError("Admin modify lottery pool failed - invalid operation. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "operation_failed");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin modify lottery pool failed - unexpected error");
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
