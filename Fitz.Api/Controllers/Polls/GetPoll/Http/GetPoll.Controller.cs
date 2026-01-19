using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Polls.Exceptions;
using Fitz.Api.Controllers.Polls.GetPoll.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Polls.GetPoll.Http;

[ApiController]
[Route("api/polls")]
public class GetPollController(GetPollFacade getPollFacade, ILogger<GetPollController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly GetPollFacade _getPollFacade = getPollFacade;
    private readonly ILogger<GetPollController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpGet("{id}")]
    [RequireDiscordAuth]
    public async Task<IActionResult> GetPoll(int id, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/polls/{id}";

        _fitzMetrics?.RecordApiRequest(endpoint, "GET");

        try
        {
            _logger.LogInformation("Get poll request received. PollId: {PollId}", id);

            var command = GetPollCommand.FromId(id);

            var response = await _getPollFacade.Execute(command, cancellationToken);

            var dto = GetPollResponseDto.From(response);

            _logger.LogInformation("Get poll completed successfully. PollId: {PollId}", dto.Id);

            return Ok(new ApiResponse<GetPollResponseDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (PollNotFound ex)
        {
            _logger.LogWarning("Get poll failed - poll not found. PollId: {PollId}", id);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get poll failed - unexpected error. PollId: {PollId}", id);
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

    [HttpGet("message/{messageId}")]
    [RequireDiscordAuth]
    public async Task<IActionResult> GetPollByMessageId(ulong messageId, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/polls/message/{messageId}";

        _fitzMetrics?.RecordApiRequest(endpoint, "GET");

        try
        {
            _logger.LogInformation("Get poll by message ID request received. MessageId: {MessageId}", messageId);

            var command = GetPollCommand.FromMessageId(messageId);

            var response = await _getPollFacade.Execute(command, cancellationToken);

            var dto = GetPollResponseDto.From(response);

            _logger.LogInformation("Get poll by message ID completed successfully. PollId: {PollId}", dto.Id);

            return Ok(new ApiResponse<GetPollResponseDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (PollNotFound ex)
        {
            _logger.LogWarning("Get poll by message ID failed - poll not found. MessageId: {MessageId}", messageId);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get poll by message ID failed - unexpected error. MessageId: {MessageId}", messageId);
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
