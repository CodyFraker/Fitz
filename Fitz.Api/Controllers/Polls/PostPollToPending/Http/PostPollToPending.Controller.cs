using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Polls.Exceptions;
using Fitz.Api.Controllers.Polls.PostPollToPending.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Polls.PostPollToPending.Http;

[ApiController]
[Route("api/polls")]
public class PostPollToPendingController(PostPollToPendingFacade postPollToPendingFacade, ILogger<PostPollToPendingController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly PostPollToPendingFacade _postPollToPendingFacade = postPollToPendingFacade;
    private readonly ILogger<PostPollToPendingController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPost("{id}/post-to-pending")]
    [RequireDiscordAuth]
    public async Task<IActionResult> PostPollToPending(int id, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/polls/{id}/post-to-pending";

        _fitzMetrics?.RecordApiRequest(endpoint, "POST");

        try
        {
            _logger.LogInformation("Post poll to pending request received. PollId: {PollId}", id);

            var command = PostPollToPendingCommand.From(id);

            var response = await _postPollToPendingFacade.Execute(command, cancellationToken);

            var dto = PostPollToPendingResponseDto.From(response);

            _logger.LogInformation("Post poll to pending completed successfully. PollId: {PollId}, MessageId: {MessageId}", id, dto.MessageId);

            return Ok(new ApiResponse<PostPollToPendingResponseDto>
            {
                Success = true,
                Message = "Poll posted to pending channel successfully",
                Data = dto
            });
        }
        catch (PollNotFound ex)
        {
            _logger.LogWarning("Post poll to pending failed - poll not found. PollId: {PollId}", id);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (PollAlreadyPostedException ex)
        {
            _logger.LogWarning("Post poll to pending failed - poll already posted. PollId: {PollId}", id);
            _fitzMetrics?.RecordApiError(endpoint, "already_posted");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidPollStatusException ex)
        {
            _logger.LogWarning("Post poll to pending failed - invalid status. PollId: {PollId}, Expected: {Expected}, Actual: {Actual}", 
                id, ex.ExpectedStatus, ex.ActualStatus);
            _fitzMetrics?.RecordApiError(endpoint, "invalid_status");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Post poll to pending failed - unexpected error. PollId: {PollId}", id);
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
