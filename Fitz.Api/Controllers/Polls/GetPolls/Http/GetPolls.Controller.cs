using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Polls.GetPolls.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Database.Entities;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Polls.GetPolls.Http;

[ApiController]
[Route("api/polls")]
public class GetPollsController(GetPollsFacade getPollsFacade, ILogger<GetPollsController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly GetPollsFacade _getPollsFacade = getPollsFacade;
    private readonly ILogger<GetPollsController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpGet]
    [RequireDiscordAuth]
    public async Task<IActionResult> GetPolls([FromQuery] PollStatusEnum? status, [FromQuery] ulong? userId, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/polls";

        _fitzMetrics?.RecordApiRequest(endpoint, "GET");

        try
        {
            _logger.LogInformation("Get polls request received. Status: {Status}, UserId: {UserId}", status, userId);

            var command = GetPollsCommand.From(status, userId);

            var response = await _getPollsFacade.Execute(command, cancellationToken);

            var dto = GetPollsResponseDto.From(response);

            _logger.LogInformation("Get polls completed successfully. Count: {Count}", dto.Polls.Count);

            return Ok(new ApiResponse<GetPollsResponseDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get polls failed - unexpected error");
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
