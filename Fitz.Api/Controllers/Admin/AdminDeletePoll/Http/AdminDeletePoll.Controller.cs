using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Admin.AdminDeletePoll.Domain;
using Fitz.Api.Controllers.Polls.Exceptions;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Admin.AdminDeletePoll.Http;

[ApiController]
[Route("api/admin/polls")]
public class AdminDeletePollController(AdminDeletePollFacade adminDeletePollFacade, ILogger<AdminDeletePollController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly AdminDeletePollFacade _adminDeletePollFacade = adminDeletePollFacade;
    private readonly ILogger<AdminDeletePollController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpDelete("{id}")]
    [RequireDiscordAuth]
    [RequireAdmin]
    public async Task<IActionResult> DeletePoll(int id, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/admin/polls/{id}";

        _fitzMetrics?.RecordApiRequest(endpoint, "DELETE");

        try
        {
            _logger.LogInformation("Admin delete poll request received. Id: {Id}", id);

            var command = AdminDeletePollCommand.From(id);

            var response = await _adminDeletePollFacade.Execute(command, cancellationToken);

            _logger.LogInformation("Admin delete poll completed successfully. Id: {Id}, Message: {Message}", id, response.Message);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = response.Message
            });
        }
        catch (PollNotFound ex)
        {
            _logger.LogWarning("Admin delete poll failed - poll not found. Id: {Id}", id);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Admin delete poll failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "validation_error");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin delete poll failed - unexpected error. Id: {Id}", id);
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
