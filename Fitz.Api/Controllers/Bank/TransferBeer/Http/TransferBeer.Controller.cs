using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Bank.TransferBeer.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Bank.TransferBeer.Http;

[ApiController]
[Route("api/bank")]
public class TransferBeerController(TransferBeerFacade transferBeerFacade, ILogger<TransferBeerController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly TransferBeerFacade _transferBeerFacade = transferBeerFacade;
    private readonly ILogger<TransferBeerController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPost("transfer")]
    [RequireDiscordAuth]
    public async Task<IActionResult> TransferBeer([FromBody] TransferBeerRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/bank/transfer";

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

            _logger.LogInformation("Transfer beer request received. SenderId: {SenderId}, RecipientId: {RecipientId}, Amount: {Amount}", request.SenderId, request.RecipientId, request.Amount);

            var command = request.ToCommand();

            var response = await _transferBeerFacade.Execute(command, cancellationToken);

            var dto = TransferBeerResponseDto.From(response);

            _logger.LogInformation("Transfer beer completed successfully. SenderId: {SenderId}, RecipientId: {RecipientId}, Amount: {Amount}", request.SenderId, request.RecipientId, dto.Amount);

            return Ok(new ApiResponse<TransferBeerResponseDto>
            {
                Success = true,
                Message = $"Transferred {dto.Amount} beer from user {dto.SenderId} to user {dto.RecipientId}.",
                Data = dto
            });
        }
        catch (AccountNotFound ex)
        {
            _logger.LogWarning("Transfer beer failed - account not found. UserId: {UserId}", ex.UserId);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Transfer beer failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "bad_request");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Transfer beer failed - insufficient beer. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "insufficient_beer");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transfer beer failed - unexpected error. SenderId: {SenderId}, RecipientId: {RecipientId}", request.SenderId, request.RecipientId);
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
