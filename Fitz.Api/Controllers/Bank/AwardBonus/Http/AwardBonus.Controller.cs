using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Bank.AwardBonus.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Bank.AwardBonus.Http;

[ApiController]
[Route("api/bank")]
public class AwardBonusController(AwardBonusFacade awardBonusFacade, ILogger<AwardBonusController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly AwardBonusFacade _awardBonusFacade = awardBonusFacade;
    private readonly ILogger<AwardBonusController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPost("award-bonus")]
    [RequireDiscordAuth]
    [RequireAdmin]
    public async Task<IActionResult> AwardBonus([FromBody] AwardBonusRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/bank/award-bonus";

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

            _logger.LogInformation("Award bonus request received. UserId: {UserId}, Amount: {Amount}", request.UserId, request.Amount);

            var command = request.ToCommand();

            var response = await _awardBonusFacade.Execute(command, cancellationToken);

            var dto = AwardBonusResponseDto.From(response);

            _logger.LogInformation("Award bonus completed successfully. UserId: {UserId}, Amount: {Amount}, NewBalance: {NewBalance}", request.UserId, dto.Amount, dto.NewBalance);

            return Ok(new ApiResponse<AwardBonusResponseDto>
            {
                Success = true,
                Message = $"Awarded {dto.Amount} beer to user {dto.UserId}.",
                Data = dto
            });
        }
        catch (AccountNotFound ex)
        {
            _logger.LogWarning("Award bonus failed - account not found. UserId: {UserId}", ex.UserId);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Award bonus failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "bad_request");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Award bonus failed - unexpected error. UserId: {UserId}", request.UserId);
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
