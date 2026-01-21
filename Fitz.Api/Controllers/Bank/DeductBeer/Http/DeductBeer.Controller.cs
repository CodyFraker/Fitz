using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Bank.DeductBeer.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Bank.DeductBeer.Http;

[ApiController]
[Route("api/bank")]
public class DeductBeerController(DeductBeerFacade deductBeerFacade, ILogger<DeductBeerController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly DeductBeerFacade _deductBeerFacade = deductBeerFacade;
    private readonly ILogger<DeductBeerController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPost("deduct-beer")]
    [RequireDiscordAuth]
    [RequireAdmin]
    public async Task<IActionResult> DeductBeer([FromBody] DeductBeerRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/bank/deduct-beer";

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

            _logger.LogInformation("Deduct beer request received. UserId: {UserId}, Amount: {Amount}, Reason: {Reason}", request.UserId, request.Amount, request.Reason);

            var command = request.ToCommand();

            var response = await _deductBeerFacade.Execute(command, cancellationToken);

            var dto = DeductBeerResponseDto.From(response);

            _logger.LogInformation("Deduct beer completed successfully. UserId: {UserId}, Amount: {Amount}, NewBalance: {NewBalance}", request.UserId, dto.Amount, dto.NewBalance);

            return Ok(new ApiResponse<DeductBeerResponseDto>
            {
                Success = true,
                Message = $"Deducted {dto.Amount} beer from user {dto.UserId}.",
                Data = dto
            });
        }
        catch (AccountNotFound ex)
        {
            _logger.LogWarning("Deduct beer failed - account not found. UserId: {UserId}", ex.UserId);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Deduct beer failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "bad_request");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Deduct beer failed - insufficient beer. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "insufficient_beer");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Deduct beer failed - unexpected error. UserId: {UserId}", request.UserId);
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
