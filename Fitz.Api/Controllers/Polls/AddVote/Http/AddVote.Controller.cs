using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Polls.AddVote.Domain;
using Fitz.Api.Controllers.Polls.Exceptions;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Polls.AddVote.Http;

[ApiController]
[Route("api/polls")]
public class AddVoteController(AddVoteFacade addVoteFacade, ILogger<AddVoteController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly AddVoteFacade _addVoteFacade = addVoteFacade;
    private readonly ILogger<AddVoteController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPost("{id}/vote")]
    [RequireDiscordAuth]
    public async Task<IActionResult> AddVote(int id, [FromBody] AddVoteRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/polls/{id}/vote";

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

            _logger.LogInformation("Add vote request received. PollId: {PollId}, UserId: {UserId}, OptionId: {OptionId}", 
                id, request.UserId, request.OptionId);

            var command = AddVoteCommand.From(id, request);

            await _addVoteFacade.Execute(command, cancellationToken);

            _logger.LogInformation("Add vote completed successfully. PollId: {PollId}, UserId: {UserId}", id, request.UserId);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Vote added successfully"
            });
        }
        catch (PollNotFound ex)
        {
            _logger.LogWarning("Add vote failed - poll not found. PollId: {PollId}", id);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (PollOptionNotFound ex)
        {
            _logger.LogWarning("Add vote failed - poll option not found. OptionId: {OptionId}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (AccountNotFound ex)
        {
            _logger.LogWarning("Add vote failed - account not found. UserId: {UserId}", ex.UserId);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Add vote failed - unexpected error. PollId: {PollId}, UserId: {UserId}", id, request.UserId);
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
