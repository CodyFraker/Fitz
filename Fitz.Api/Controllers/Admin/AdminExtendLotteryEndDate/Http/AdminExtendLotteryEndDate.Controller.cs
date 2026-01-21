using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Admin.AdminExtendLotteryEndDate.Domain;
using Fitz.Api.Controllers.Lottery.Exceptions;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Admin.AdminExtendLotteryEndDate.Http;

[ApiController]
[Route("api/admin/lottery")]
public class AdminExtendLotteryEndDateController(AdminExtendLotteryEndDateFacade adminExtendLotteryEndDateFacade, ILogger<AdminExtendLotteryEndDateController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly AdminExtendLotteryEndDateFacade _adminExtendLotteryEndDateFacade = adminExtendLotteryEndDateFacade;
    private readonly ILogger<AdminExtendLotteryEndDateController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPatch("current/end-date")]
    [RequireDiscordAuth]
    [RequireAdmin]
    public async Task<IActionResult> ExtendLotteryEndDate([FromBody] AdminExtendLotteryEndDateRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/admin/lottery/current/end-date";

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

            _logger.LogInformation("Admin extend lottery end date request received. EndDate: {EndDate}", request.EndDate);

            var command = request.ToCommand();

            var response = await _adminExtendLotteryEndDateFacade.Execute(command, cancellationToken);

            _logger.LogInformation("Admin extend lottery end date completed successfully. Message: {Message}", response.Message);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = response.Message
            });
        }
        catch (LotteryNotFound ex)
        {
            _logger.LogWarning("Admin extend lottery end date failed - lottery not found");
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Admin extend lottery end date failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "validation_error");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin extend lottery end date failed - unexpected error");
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
