using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Account.SetSafeBalance.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Account.SetSafeBalance.Http;

[ApiController]
[Route("api/account")]
public class SetSafeBalanceController(SetSafeBalanceFacade setSafeBalanceFacade, ILogger<SetSafeBalanceController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly SetSafeBalanceFacade _setSafeBalanceFacade = setSafeBalanceFacade;
    private readonly ILogger<SetSafeBalanceController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPost("safe-balance")]
    [RequireDiscordAuth]
    public async Task<IActionResult> SetSafeBalance([FromBody] SetSafeBalanceRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/account/safe-balance";

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

            _logger.LogInformation("Set safe balance request received. UserId: {UserId}, SafeBalance: {SafeBalance}", request.UserId, request.SafeBalance);

            var command = request.ToCommand();

            var response = await _setSafeBalanceFacade.Execute(command, cancellationToken);

            var dto = SetSafeBalanceResponseDto.From(response);

            _logger.LogInformation("Set safe balance completed successfully. UserId: {UserId}, SafeBalance: {SafeBalance}", request.UserId, dto.SafeBalance);

            return Ok(new ApiResponse<SetSafeBalanceResponseDto>
            {
                Success = true,
                Message = "Safe balance updated successfully.",
                Data = dto
            });
        }
        catch (AccountNotFound ex)
        {
            _logger.LogWarning("Set safe balance failed - account not found. UserId: {UserId}", ex.UserId);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Set safe balance failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "bad_request");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Set safe balance failed - unexpected error. UserId: {UserId}", request.UserId);
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
