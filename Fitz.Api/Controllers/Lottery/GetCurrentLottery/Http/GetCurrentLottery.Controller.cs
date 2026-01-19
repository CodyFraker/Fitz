using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Lottery.Exceptions;
using Fitz.Api.Controllers.Lottery.GetCurrentLottery.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Lottery.GetCurrentLottery.Http;

[ApiController]
[Route("api/lottery")]
public class GetCurrentLotteryController(GetCurrentLotteryFacade getCurrentLotteryFacade, ILogger<GetCurrentLotteryController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly GetCurrentLotteryFacade _getCurrentLotteryFacade = getCurrentLotteryFacade;
    private readonly ILogger<GetCurrentLotteryController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpGet("current")]
    [RequireDiscordAuth]
    public async Task<IActionResult> GetCurrentLottery(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/lottery/current";

        _fitzMetrics?.RecordApiRequest(endpoint, "GET");

        try
        {
            _logger.LogInformation("Get current lottery request received");

            var command = GetCurrentLotteryCommand.From();

            var response = await _getCurrentLotteryFacade.Execute(command, cancellationToken);

            var dto = GetCurrentLotteryResponseDto.From(response);

            _logger.LogInformation("Get current lottery completed successfully. LotteryId: {LotteryId}", dto.Id);

            return Ok(new ApiResponse<GetCurrentLotteryResponseDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (LotteryNotFound ex)
        {
            _logger.LogWarning("Get current lottery failed - lottery not found");
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Get current lottery failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "bad_request");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get current lottery failed - unexpected error");
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
