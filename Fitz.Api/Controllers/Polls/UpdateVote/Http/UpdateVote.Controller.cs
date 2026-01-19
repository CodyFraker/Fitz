using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Polls.Exceptions;
using Fitz.Api.Controllers.Polls.UpdateVote.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Polls.UpdateVote.Http;

[ApiController]
[Route("api/polls")]
public class UpdateVoteController(UpdateVoteFacade updateVoteFacade, ILogger<UpdateVoteController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly UpdateVoteFacade _updateVoteFacade = updateVoteFacade;
    private readonly ILogger<UpdateVoteController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPut("{id}/vote")]
    [RequireDiscordAuth]
    public async Task<IActionResult> UpdateVote(int id, [FromBody] UpdateVoteRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/polls/{id}/vote";

        _fitzMetrics?.RecordApiRequest(endpoint, "PUT");

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

            _logger.LogInformation("Update vote request received. PollId: {PollId}, UserId: {UserId}, OptionId: {OptionId}", 
                id, request.UserId, request.OptionId);

            var command = UpdateVoteCommand.From(id, request);

            var response = await _updateVoteFacade.Execute(command, cancellationToken);

            var dto = UpdateVoteResponseDto.From(response);

            _logger.LogInformation("Update vote completed successfully. PollId: {PollId}, UserId: {UserId}", id, request.UserId);

            return Ok(new ApiResponse<UpdateVoteResponseDto>
            {
                Success = true,
                Message = "Vote updated successfully",
                Data = dto
            });
        }
        catch (PollNotFound ex)
        {
            _logger.LogWarning("Update vote failed - poll not found. PollId: {PollId}", id);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (VoteNotFound ex)
        {
            _logger.LogWarning("Update vote failed - vote not found. PollId: {PollId}, UserId: {UserId}", id, request.UserId);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (AccountNotFound ex)
        {
            _logger.LogWarning("Update vote failed - account not found. UserId: {UserId}", ex.UserId);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update vote failed - unexpected error. PollId: {PollId}, UserId: {UserId}", id, request.UserId);
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
