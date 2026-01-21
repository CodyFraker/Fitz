using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Bank.GetTopBalances.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Bank.GetTopBalances.Http;

[ApiController]
[Route("api/bank")]
public class GetTopBalancesController(GetTopBalancesFacade getTopBalancesFacade, ILogger<GetTopBalancesController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly GetTopBalancesFacade _getTopBalancesFacade = getTopBalancesFacade;
    private readonly ILogger<GetTopBalancesController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpGet("top-balances")]
    [RequireDiscordAuth]
    public async Task<IActionResult> GetTopBalances([FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/bank/top-balances";

        _fitzMetrics?.RecordApiRequest(endpoint, "GET");

        try
        {
            _logger.LogInformation("Get top balances request received. Limit: {Limit}", limit);

            var command = GetTopBalancesCommand.From(limit);

            var response = await _getTopBalancesFacade.Execute(command, cancellationToken);

            var dto = GetTopBalancesResponseDto.From(response);

            _logger.LogInformation("Get top balances completed successfully. Count: {Count}", dto.Accounts.Count);

            return Ok(new ApiResponse<GetTopBalancesResponseDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Get top balances failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "bad_request");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get top balances failed - unexpected error");
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
