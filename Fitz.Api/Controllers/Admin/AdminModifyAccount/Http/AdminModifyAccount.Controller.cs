using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Admin.AdminModifyAccount.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Admin.AdminModifyAccount.Http;

[ApiController]
[Route("api/admin/accounts")]
public class AdminModifyAccountController(AdminModifyAccountFacade adminModifyAccountFacade, ILogger<AdminModifyAccountController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly AdminModifyAccountFacade _adminModifyAccountFacade = adminModifyAccountFacade;
    private readonly ILogger<AdminModifyAccountController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPatch("{userId}")]
    [RequireDiscordAuth]
    [RequireAdmin]
    public async Task<IActionResult> ModifyAccount(ulong userId, [FromBody] AdminModifyAccountRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/admin/accounts/{userId}";

        _fitzMetrics?.RecordApiRequest(endpoint, "PATCH");

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

            if (request.UserId != userId)
            {
                _fitzMetrics?.RecordApiError(endpoint, "validation_error");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "UserId in path must match UserId in request body"
                });
            }

            _logger.LogInformation("Admin modify account request received. UserId: {UserId}", userId);

            var command = AdminModifyAccountCommand.From(userId, request);

            var response = await _adminModifyAccountFacade.Execute(command, cancellationToken);

            var dto = AdminModifyAccountResponseDto.From(response);

            _logger.LogInformation("Admin modify account completed successfully. UserId: {UserId}", userId);

            return Ok(new ApiResponse<AdminModifyAccountResponseDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (AccountNotFound ex)
        {
            _logger.LogWarning("Admin modify account failed - account not found. UserId: {UserId}", ex.UserId);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Admin modify account failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "validation_error");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError("Admin modify account failed - invalid operation. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "business_error");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin modify account failed - unexpected error. UserId: {UserId}", userId);
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
