using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Lottery.BuyTickets.Domain;
using Fitz.Api.Controllers.Lottery.Exceptions;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Lottery.BuyTickets.Http;

[ApiController]
[Route("api/lottery")]
public class BuyTicketsController(BuyTicketsFacade buyTicketsFacade, ILogger<BuyTicketsController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly BuyTicketsFacade _buyTicketsFacade = buyTicketsFacade;
    private readonly ILogger<BuyTicketsController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPost("buy-tickets")]
    [RequireDiscordAuth]
    public async Task<IActionResult> BuyTickets([FromBody] BuyTicketsRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/lottery/buy-tickets";

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

            _logger.LogInformation("Buy tickets request received. UserId: {UserId}, Amount: {Amount}", request.UserId, request.Amount);

            var command = request.ToCommand();

            var response = await _buyTicketsFacade.Execute(command, cancellationToken);

            var dto = BuyTicketsResponseDto.From(response);

            _logger.LogInformation("Buy tickets completed successfully. UserId: {UserId}, TicketsPurchased: {TicketsPurchased}", 
                request.UserId, dto.TicketsPurchased);

            return Ok(new ApiResponse<BuyTicketsResponseDto>
            {
                Success = true,
                Message = $"Successfully purchased {dto.TicketsPurchased} ticket(s).",
                Data = dto
            });
        }
        catch (AccountNotFound ex)
        {
            _logger.LogWarning("Buy tickets failed - account not found. UserId: {UserId}", ex.UserId);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (LotteryNotFound ex)
        {
            _logger.LogWarning("Buy tickets failed - lottery not found");
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InsufficientBeerException ex)
        {
            _logger.LogWarning("Buy tickets failed - insufficient beer. Required: {Required}, Current: {Current}", 
                ex.RequiredAmount, ex.CurrentBalance);
            _fitzMetrics?.RecordApiError(endpoint, "insufficient_beer");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (MaxTicketsReachedException ex)
        {
            _logger.LogWarning("Buy tickets failed - max tickets reached. Current: {Current}, Max: {Max}", 
                ex.CurrentTicketCount, ex.MaxTickets);
            _fitzMetrics?.RecordApiError(endpoint, "max_tickets_reached");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidTicketAmountException ex)
        {
            _logger.LogWarning("Buy tickets failed - invalid ticket amount. Amount: {Amount}, Reason: {Reason}", 
                ex.RequestedAmount, ex.Reason);
            _fitzMetrics?.RecordApiError(endpoint, "invalid_ticket_amount");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Buy tickets failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "bad_request");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Buy tickets failed - unexpected error. UserId: {UserId}", request.UserId);
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
