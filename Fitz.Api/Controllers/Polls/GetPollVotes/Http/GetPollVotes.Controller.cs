using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Polls.Exceptions;
using Fitz.Api.Controllers.Polls.GetPollVotes.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Polls.GetPollVotes.Http;

[ApiController]
[Route("api/polls")]
public class GetPollVotesController(GetPollVotesFacade getPollVotesFacade, ILogger<GetPollVotesController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly GetPollVotesFacade _getPollVotesFacade = getPollVotesFacade;
    private readonly ILogger<GetPollVotesController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpGet("{id}/votes")]
    [RequireDiscordAuth]
    public async Task<IActionResult> GetPollVotes(int id, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/polls/{id}/votes";

        _fitzMetrics?.RecordApiRequest(endpoint, "GET");

        try
        {
            _logger.LogInformation("Get poll votes request received. PollId: {PollId}", id);

            var command = GetPollVotesCommand.From(id);

            var response = await _getPollVotesFacade.Execute(command, cancellationToken);

            var dto = GetPollVotesResponseDto.From(response);

            _logger.LogInformation("Get poll votes completed successfully. PollId: {PollId}, Count: {Count}", id, dto.Votes.Count);

            return Ok(new ApiResponse<GetPollVotesResponseDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (PollNotFound ex)
        {
            _logger.LogWarning("Get poll votes failed - poll not found. PollId: {PollId}", id);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get poll votes failed - unexpected error. PollId: {PollId}", id);
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
