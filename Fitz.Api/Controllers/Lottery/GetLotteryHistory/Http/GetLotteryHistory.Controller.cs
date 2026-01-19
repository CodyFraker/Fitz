using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Lottery.GetLotteryHistory.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Lottery.GetLotteryHistory.Http;

[ApiController]
[Route("api/lottery")]
public class GetLotteryHistoryController(GetLotteryHistoryFacade getLotteryHistoryFacade, ILogger<GetLotteryHistoryController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly GetLotteryHistoryFacade _getLotteryHistoryFacade = getLotteryHistoryFacade;
    private readonly ILogger<GetLotteryHistoryController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpGet("history")]
    [RequireDiscordAuth]
    public async Task<IActionResult> GetLotteryHistory([FromQuery] int skip = 0, [FromQuery] int take = 10, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/lottery/history";

        _fitzMetrics?.RecordApiRequest(endpoint, "GET");

        try
        {
            _logger.LogInformation("Get lottery history request received. Skip: {Skip}, Take: {Take}", skip, take);

            var command = GetLotteryHistoryCommand.From(skip, take);

            var response = await _getLotteryHistoryFacade.Execute(command, cancellationToken);

            var dto = GetLotteryHistoryResponseDto.From(response);

            _logger.LogInformation("Get lottery history completed successfully. TotalCount: {TotalCount}, ItemsReturned: {ItemsReturned}", dto.TotalCount, dto.Lotteries.Count);

            return Ok(new ApiResponse<GetLotteryHistoryResponseDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Get lottery history failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "bad_request");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get lottery history failed - unexpected error");
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
