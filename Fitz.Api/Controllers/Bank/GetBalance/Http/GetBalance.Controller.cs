using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Bank.GetBalance.Domain;
using Fitz.Api.Extensions;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Bank.GetBalance.Http;

[ApiController]
[Route("api/bank")]
public class GetBalanceController(GetBalanceFacade getBalanceFacade, ILogger<GetBalanceController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly GetBalanceFacade _getBalanceFacade = getBalanceFacade;
    private readonly ILogger<GetBalanceController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpGet("balance/{userId}")]
    [RequireDiscordAuth]
    [RequireOwnData]
    public async Task<IActionResult> GetBalance(ulong userId, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/bank/balance";

        _fitzMetrics?.RecordApiRequest(endpoint, "GET");

        try
        {
            _logger.LogInformation("Get balance request received. UserId: {UserId}", userId);

            var username = User.GetDiscordUsername();
            var command = GetBalanceCommand.From(userId, username);

            var response = await _getBalanceFacade.Execute(command, cancellationToken);

            var dto = GetBalanceResponseDto.From(response);

            _logger.LogInformation("Get balance completed successfully. UserId: {UserId}, Beer: {Beer}", userId, dto.Beer);

            return Ok(new ApiResponse<GetBalanceResponseDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Get balance failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "bad_request");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get balance failed - unexpected error. UserId: {UserId}", userId);
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
