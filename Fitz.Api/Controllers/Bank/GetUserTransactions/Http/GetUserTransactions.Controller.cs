using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Bank.GetUserTransactions.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Bank.GetUserTransactions.Http;

[ApiController]
[Route("api/bank")]
public class GetUserTransactionsController(GetUserTransactionsFacade getUserTransactionsFacade, ILogger<GetUserTransactionsController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly GetUserTransactionsFacade _getUserTransactionsFacade = getUserTransactionsFacade;
    private readonly ILogger<GetUserTransactionsController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpGet("transactions/{userId}")]
    [RequireDiscordAuth]
    [RequireOwnData]
    public async Task<IActionResult> GetUserTransactions(ulong userId, [FromQuery] int skip = 0, [FromQuery] int take = 10, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/bank/transactions";

        _fitzMetrics?.RecordApiRequest(endpoint, "GET");

        try
        {
            _logger.LogInformation("Get user transactions request received. UserId: {UserId}, Skip: {Skip}, Take: {Take}", userId, skip, take);

            var command = GetUserTransactionsCommand.From(userId, skip, take);

            var response = await _getUserTransactionsFacade.Execute(command, cancellationToken);

            var dto = GetUserTransactionsResponseDto.From(response, skip, take);

            _logger.LogInformation("Get user transactions completed successfully. Count: {Count}, TotalCount: {TotalCount}", dto.Transactions.Count, dto.TotalCount);

            return Ok(new ApiResponse<GetUserTransactionsResponseDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Get user transactions failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "bad_request");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get user transactions failed - unexpected error. UserId: {UserId}", userId);
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
