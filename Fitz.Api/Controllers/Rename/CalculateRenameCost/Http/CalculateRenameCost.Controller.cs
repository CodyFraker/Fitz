using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Rename.CalculateRenameCost.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Rename.CalculateRenameCost.Http;

[ApiController]
[Route("api/rename")]
public class CalculateRenameCostController(CalculateRenameCostFacade calculateRenameCostFacade, ILogger<CalculateRenameCostController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly CalculateRenameCostFacade _calculateRenameCostFacade = calculateRenameCostFacade;
    private readonly ILogger<CalculateRenameCostController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPost("calculate-cost")]
    [RequireDiscordAuth]
    public async Task<IActionResult> CalculateRenameCost([FromBody] CalculateRenameCostRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/rename/calculate-cost";

        _fitzMetrics?.RecordApiRequest(endpoint, "POST");

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

            _logger.LogInformation("Calculate rename cost request received. AffectedUserId: {AffectedUserId}, RequestedUserId: {RequestedUserId}", 
                request.AffectedUserId, request.RequestedUserId);

            var command = request.ToCommand();

            var response = await _calculateRenameCostFacade.Execute(command, cancellationToken);

            var dto = CalculateRenameCostResponseDto.From(response);

            _logger.LogInformation("Calculate rename cost completed successfully. Cost: {Cost}", dto.Cost);

            return Ok(new ApiResponse<CalculateRenameCostResponseDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (AccountNotFound ex)
        {
            _logger.LogWarning("Calculate rename cost failed - account not found. UserId: {UserId}", ex.UserId);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Calculate rename cost failed - unexpected error. AffectedUserId: {AffectedUserId}, RequestedUserId: {RequestedUserId}", 
                request.AffectedUserId, request.RequestedUserId);
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
