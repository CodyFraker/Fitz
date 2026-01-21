using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Account.SetTicketAmount.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Account.SetTicketAmount.Http;

[ApiController]
[Route("api/account")]
public class SetTicketAmountController(SetTicketAmountFacade setTicketAmountFacade, ILogger<SetTicketAmountController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly SetTicketAmountFacade _setTicketAmountFacade = setTicketAmountFacade;
    private readonly ILogger<SetTicketAmountController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPost("ticket-amount")]
    [RequireDiscordAuth]
    public async Task<IActionResult> SetTicketAmount([FromBody] SetTicketAmountRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/account/ticket-amount";

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

            _logger.LogInformation("Set ticket amount request received. UserId: {UserId}, Amount: {Amount}", request.UserId, request.Amount);

            var command = request.ToCommand();

            var response = await _setTicketAmountFacade.Execute(command, cancellationToken);

            var dto = SetTicketAmountResponseDto.From(response);

            _logger.LogInformation("Set ticket amount completed successfully. UserId: {UserId}, Amount: {Amount}", request.UserId, dto.Amount);

            return Ok(new ApiResponse<SetTicketAmountResponseDto>
            {
                Success = true,
                Message = "Ticket amount updated successfully.",
                Data = dto
            });
        }
        catch (AccountNotFound ex)
        {
            _logger.LogWarning("Set ticket amount failed - account not found. UserId: {UserId}", ex.UserId);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Set ticket amount failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "bad_request");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Set ticket amount failed - unexpected error. UserId: {UserId}", request.UserId);
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
