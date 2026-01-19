using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Polls.Exceptions;
using Fitz.Api.Controllers.Polls.GetPollOptions.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Polls.GetPollOptions.Http;

[ApiController]
[Route("api/polls")]
public class GetPollOptionsController(GetPollOptionsFacade getPollOptionsFacade, ILogger<GetPollOptionsController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly GetPollOptionsFacade _getPollOptionsFacade = getPollOptionsFacade;
    private readonly ILogger<GetPollOptionsController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpGet("{id}/options")]
    [RequireDiscordAuth]
    public async Task<IActionResult> GetPollOptions(int id, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/polls/{id}/options";

        _fitzMetrics?.RecordApiRequest(endpoint, "GET");

        try
        {
            _logger.LogInformation("Get poll options request received. PollId: {PollId}", id);

            var command = GetPollOptionsCommand.From(id);

            var response = await _getPollOptionsFacade.Execute(command, cancellationToken);

            var dto = GetPollOptionsResponseDto.From(response);

            _logger.LogInformation("Get poll options completed successfully. PollId: {PollId}, Count: {Count}", id, dto.Options.Count);

            return Ok(new ApiResponse<GetPollOptionsResponseDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (PollNotFound ex)
        {
            _logger.LogWarning("Get poll options failed - poll not found. PollId: {PollId}", id);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get poll options failed - unexpected error. PollId: {PollId}", id);
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
