using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Bank.GetTransactions.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Bank.GetTransactions.Http;

[ApiController]
[Route("api/bank")]
public class GetTransactionsController(GetTransactionsFacade getTransactionsFacade, ILogger<GetTransactionsController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly GetTransactionsFacade _getTransactionsFacade = getTransactionsFacade;
    private readonly ILogger<GetTransactionsController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpGet("transactions")]
    [RequireDiscordAuth]
    public async Task<IActionResult> GetTransactions([FromQuery] int take = 10, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/bank/transactions";

        _fitzMetrics?.RecordApiRequest(endpoint, "GET");

        try
        {
            _logger.LogInformation("Get transactions request received. Take: {Take}", take);

            var command = GetTransactionsCommand.From(take);

            var response = await _getTransactionsFacade.Execute(command, cancellationToken);

            var dto = GetTransactionsResponseDto.From(response);

            _logger.LogInformation("Get transactions completed successfully. Count: {Count}", dto.Transactions.Count);

            return Ok(new ApiResponse<GetTransactionsResponseDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Get transactions failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "bad_request");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get transactions failed - unexpected error");
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
