using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Admin.AdminBulkUpdateFavorability.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Admin.AdminBulkUpdateFavorability.Http;

[ApiController]
[Route("api/admin/favorability")]
public class AdminBulkUpdateFavorabilityController(AdminBulkUpdateFavorabilityFacade adminBulkUpdateFavorabilityFacade, ILogger<AdminBulkUpdateFavorabilityController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly AdminBulkUpdateFavorabilityFacade _adminBulkUpdateFavorabilityFacade = adminBulkUpdateFavorabilityFacade;
    private readonly ILogger<AdminBulkUpdateFavorabilityController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPost("users/bulk")]
    [RequireDiscordAuth]
    [RequireAdmin]
    public async Task<IActionResult> BulkUpdateFavorability([FromBody] BulkUpdateFavorabilityRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/admin/favorability/users/bulk";

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

            _logger.LogInformation("Admin bulk update favorability request received. UserIdsCount: {UserIdsCount}, Favorability: {Favorability}", 
                request.UserIds?.Length ?? 0, request.Favorability);

            var command = request.ToCommand();

            var response = await _adminBulkUpdateFavorabilityFacade.Execute(command, cancellationToken);

            _logger.LogInformation("Admin bulk update favorability completed successfully. SuccessCount: {SuccessCount}, FailCount: {FailCount}", 
                response.SuccessCount, response.FailCount);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = response.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Admin bulk update favorability failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "validation_error");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin bulk update favorability failed - unexpected error");
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
