using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Rename.Exceptions;
using Fitz.Api.Controllers.Rename.UpdateRenameStatus.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Rename.UpdateRenameStatus.Http;

[ApiController]
[Route("api/rename")]
public class UpdateRenameStatusController(UpdateRenameStatusFacade updateRenameStatusFacade, ILogger<UpdateRenameStatusController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly UpdateRenameStatusFacade _updateRenameStatusFacade = updateRenameStatusFacade;
    private readonly ILogger<UpdateRenameStatusController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPatch("{id}/status")]
    [RequireDiscordAuth]
    public async Task<IActionResult> UpdateRenameStatus(int id, [FromBody] UpdateRenameStatusRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/rename/{id}/status";

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

            _logger.LogInformation("Update rename status request received. RenameId: {RenameId}, Status: {Status}", id, request.Status);

            var command = UpdateRenameStatusCommand.From(id, request);

            var response = await _updateRenameStatusFacade.Execute(command, cancellationToken);

            var dto = UpdateRenameStatusResponseDto.From(response);

            _logger.LogInformation("Update rename status completed successfully. RenameId: {RenameId}, Status: {Status}", id, request.Status);

            return Ok(new ApiResponse<UpdateRenameStatusResponseDto>
            {
                Success = true,
                Message = "Rename status updated successfully",
                Data = dto
            });
        }
        catch (RenameNotFound ex)
        {
            _logger.LogWarning("Update rename status failed - rename not found. RenameId: {RenameId}", id);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update rename status failed - unexpected error. RenameId: {RenameId}", id);
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
