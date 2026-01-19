using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Lottery.GetLotteryStatistics.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Lottery.GetLotteryStatistics.Http;

[ApiController]
[Route("api/lottery")]
public class GetLotteryStatisticsController(GetLotteryStatisticsFacade getLotteryStatisticsFacade, ILogger<GetLotteryStatisticsController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly GetLotteryStatisticsFacade _getLotteryStatisticsFacade = getLotteryStatisticsFacade;
    private readonly ILogger<GetLotteryStatisticsController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpGet("statistics")]
    [RequireDiscordAuth]
    public async Task<IActionResult> GetLotteryStatistics(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/lottery/statistics";

        _fitzMetrics?.RecordApiRequest(endpoint, "GET");

        try
        {
            _logger.LogInformation("Get lottery statistics request received");

            var command = GetLotteryStatisticsCommand.From();

            var response = await _getLotteryStatisticsFacade.Execute(command, cancellationToken);

            var dto = GetLotteryStatisticsResponseDto.From(response);

            _logger.LogInformation("Get lottery statistics completed successfully. DataPointsCount: {DataPointsCount}", dto.DataPoints.Count);

            return Ok(new ApiResponse<GetLotteryStatisticsResponseDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Get lottery statistics failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "bad_request");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get lottery statistics failed - unexpected error");
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
