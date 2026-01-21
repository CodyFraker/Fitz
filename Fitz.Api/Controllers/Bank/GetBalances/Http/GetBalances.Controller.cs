using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Bank.GetBalances.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Bank.GetBalances.Http;

[ApiController]
[Route("api/bank")]
public class GetBalancesController(GetBalancesFacade getBalancesFacade, ILogger<GetBalancesController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly GetBalancesFacade _getBalancesFacade = getBalancesFacade;
    private readonly ILogger<GetBalancesController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpGet("balances")]
    [RequireDiscordAuth]
    public async Task<IActionResult> GetBalances([FromQuery] int skip = 0, [FromQuery] int take = 10, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/bank/balances";

        _fitzMetrics?.RecordApiRequest(endpoint, "GET");

        try
        {
            _logger.LogInformation("Get balances request received. Skip: {Skip}, Take: {Take}", skip, take);

            var command = GetBalancesCommand.From(skip, take);

            var response = await _getBalancesFacade.Execute(command, cancellationToken);

            var dto = GetBalancesResponseDto.From(response, skip, take);

            _logger.LogInformation("Get balances completed successfully. Count: {Count}, TotalCount: {TotalCount}", dto.Accounts.Count, dto.TotalCount);

            return Ok(new ApiResponse<GetBalancesResponseDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Get balances failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "bad_request");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get balances failed - unexpected error");
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
