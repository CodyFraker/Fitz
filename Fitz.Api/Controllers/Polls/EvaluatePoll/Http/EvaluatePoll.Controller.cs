using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Polls.Exceptions;
using Fitz.Api.Controllers.Polls.EvaluatePoll.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Polls.EvaluatePoll.Http;

[ApiController]
[Route("api/polls")]
public class EvaluatePollController(EvaluatePollFacade evaluatePollFacade, ILogger<EvaluatePollController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly EvaluatePollFacade _evaluatePollFacade = evaluatePollFacade;
    private readonly ILogger<EvaluatePollController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPatch("{id}/evaluate")]
    [RequireDiscordAuth]
    [RequireAdmin]
    public async Task<IActionResult> EvaluatePoll(int id, [FromBody] EvaluatePollRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/polls/{id}/evaluate";

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

            _logger.LogInformation("Evaluate poll request received. PollId: {PollId}, Status: {Status}", id, request.Status);

            var command = EvaluatePollCommand.From(id, request);

            var response = await _evaluatePollFacade.Execute(command, cancellationToken);

            var dto = EvaluatePollResponseDto.From(response);

            _logger.LogInformation("Evaluate poll completed successfully. PollId: {PollId}, Status: {Status}", id, request.Status);

            return Ok(new ApiResponse<EvaluatePollResponseDto>
            {
                Success = true,
                Message = $"Poll #{dto.Id} {dto.Status}.",
                Data = dto
            });
        }
        catch (PollNotFound ex)
        {
            _logger.LogWarning("Evaluate poll failed - poll not found. PollId: {PollId}", id);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Evaluate poll failed - unexpected error. PollId: {PollId}", id);
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
