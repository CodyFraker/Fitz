using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Account.DeactivateAccount.Domain;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Extensions;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Account.DeactivateAccount.Http;

[ApiController]
[Route("api/account")]
public class DeactivateAccountController(DeactivateAccountFacade deactivateAccountFacade, ILogger<DeactivateAccountController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly DeactivateAccountFacade _deactivateAccountFacade = deactivateAccountFacade;
    private readonly ILogger<DeactivateAccountController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPost("deactivate")]
    [RequireDiscordAuth]
    public async Task<IActionResult> DeactivateAccount([FromBody] DeactivateAccountRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/account/deactivate";

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

            var authenticatedUserId = User.GetDiscordUserId();
            if (authenticatedUserId == null || authenticatedUserId.Value != request.UserId)
            {
                _logger.LogWarning("Deactivate account failed - user cannot deactivate another user's account. AuthenticatedUserId: {AuthenticatedUserId}, RequestUserId: {RequestUserId}", authenticatedUserId, request.UserId);
                _fitzMetrics?.RecordApiError(endpoint, "forbidden");
                return Forbid();
            }

            _logger.LogInformation("Deactivate account request received. UserId: {UserId}", request.UserId);

            var command = request.ToCommand();

            var response = await _deactivateAccountFacade.Execute(command, cancellationToken);

            var dto = DeactivateAccountResponseDto.From(response);

            _logger.LogInformation("Deactivate account completed successfully. UserId: {UserId}, Deactivated: {Deactivated}", request.UserId, dto.Deactivated);

            return Ok(new ApiResponse<DeactivateAccountResponseDto>
            {
                Success = true,
                Message = "Account deactivated successfully.",
                Data = dto
            });
        }
        catch (AccountNotFound ex)
        {
            _logger.LogWarning("Deactivate account failed - account not found. UserId: {UserId}", ex.UserId);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Deactivate account failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "bad_request");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Deactivate account failed - unexpected error. UserId: {UserId}", request.UserId);
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
