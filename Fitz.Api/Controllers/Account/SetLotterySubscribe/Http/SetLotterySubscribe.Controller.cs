using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Account.SetLotterySubscribe.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Account.SetLotterySubscribe.Http;

[ApiController]
[Route("api/account")]
public class SetLotterySubscribeController(SetLotterySubscribeFacade setLotterySubscribeFacade, ILogger<SetLotterySubscribeController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly SetLotterySubscribeFacade _setLotterySubscribeFacade = setLotterySubscribeFacade;
    private readonly ILogger<SetLotterySubscribeController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPost("lottery-subscribe")]
    [RequireDiscordAuth]
    public async Task<IActionResult> SetLotterySubscribe([FromBody] SetLotterySubscribeRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/account/lottery-subscribe";

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

            _logger.LogInformation("Set lottery subscribe request received. UserId: {UserId}, Subscribe: {Subscribe}", request.UserId, request.Subscribe);

            var command = request.ToCommand();

            var response = await _setLotterySubscribeFacade.Execute(command, cancellationToken);

            var dto = SetLotterySubscribeResponseDto.From(response);

            _logger.LogInformation("Set lottery subscribe completed successfully. UserId: {UserId}, Subscribe: {Subscribe}", request.UserId, dto.Subscribe);

            return Ok(new ApiResponse<SetLotterySubscribeResponseDto>
            {
                Success = true,
                Message = "Lottery subscription updated successfully.",
                Data = dto
            });
        }
        catch (AccountNotFound ex)
        {
            _logger.LogWarning("Set lottery subscribe failed - account not found. UserId: {UserId}", ex.UserId);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Set lottery subscribe failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "bad_request");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Set lottery subscribe failed - unexpected error. UserId: {UserId}", request.UserId);
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
