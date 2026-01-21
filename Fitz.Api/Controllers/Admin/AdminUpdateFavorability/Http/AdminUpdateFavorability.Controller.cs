using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Admin.AdminUpdateFavorability.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Admin.AdminUpdateFavorability.Http;

[ApiController]
[Route("api/admin/favorability")]
public class AdminUpdateFavorabilityController(AdminUpdateFavorabilityFacade adminUpdateFavorabilityFacade, ILogger<AdminUpdateFavorabilityController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly AdminUpdateFavorabilityFacade _adminUpdateFavorabilityFacade = adminUpdateFavorabilityFacade;
    private readonly ILogger<AdminUpdateFavorabilityController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPatch("users/{userId}")]
    [RequireDiscordAuth]
    [RequireAdmin]
    public async Task<IActionResult> UpdateFavorability(ulong userId, [FromBody] UpdateFavorabilityRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/admin/favorability/users/{userId}";

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

            _logger.LogInformation("Admin update favorability request received. UserId: {UserId}, Favorability: {Favorability}", 
                userId, request.Favorability);

            var command = AdminUpdateFavorabilityCommand.From(userId, request);

            var response = await _adminUpdateFavorabilityFacade.Execute(command, cancellationToken);

            _logger.LogInformation("Admin update favorability completed successfully. Message: {Message}", response.Message);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = response.Message
            });
        }
        catch (AccountNotFound ex)
        {
            _logger.LogWarning("Admin update favorability failed - account not found. UserId: {UserId}", ex.UserId);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Admin update favorability failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "validation_error");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError("Admin update favorability failed - invalid operation. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "business_error");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin update favorability failed - unexpected error. UserId: {UserId}", userId);
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
