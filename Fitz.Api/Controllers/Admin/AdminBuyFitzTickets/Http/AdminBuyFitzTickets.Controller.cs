using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Admin.AdminBuyFitzTickets.Domain;
using Fitz.Api.Controllers.Lottery.Exceptions;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Admin.AdminBuyFitzTickets.Http;

[ApiController]
[Route("api/admin/lottery")]
public class AdminBuyFitzTicketsController(AdminBuyFitzTicketsFacade adminBuyFitzTicketsFacade, ILogger<AdminBuyFitzTicketsController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly AdminBuyFitzTicketsFacade _adminBuyFitzTicketsFacade = adminBuyFitzTicketsFacade;
    private readonly ILogger<AdminBuyFitzTicketsController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPost("current/fitz-tickets")]
    [RequireDiscordAuth]
    [RequireAdmin]
    public async Task<IActionResult> BuyFitzTickets([FromBody] AdminBuyFitzTicketsRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/admin/lottery/current/fitz-tickets";

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

            _logger.LogInformation("Admin buy Fitz tickets request received. Tickets: {Tickets}", request.Tickets);

            var command = request.ToCommand();

            var response = await _adminBuyFitzTicketsFacade.Execute(command, cancellationToken);

            _logger.LogInformation("Admin buy Fitz tickets completed successfully. Message: {Message}", response.Message);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = response.Message
            });
        }
        catch (LotteryNotFound ex)
        {
            _logger.LogWarning("Admin buy Fitz tickets failed - lottery not found");
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Admin buy Fitz tickets failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "validation_error");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError("Admin buy Fitz tickets failed - invalid operation. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "operation_failed");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin buy Fitz tickets failed - unexpected error");
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
