using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Polls.GetPollsWithDetails.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Database.Entities;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Polls.GetPollsWithDetails.Http;

[ApiController]
[Route("api/polls")]
public class GetPollsWithDetailsController(GetPollsWithDetailsFacade getPollsWithDetailsFacade, ILogger<GetPollsWithDetailsController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly GetPollsWithDetailsFacade _getPollsWithDetailsFacade = getPollsWithDetailsFacade;
    private readonly ILogger<GetPollsWithDetailsController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpGet("with-details")]
    [RequireDiscordAuth]
    public async Task<IActionResult> GetPollsWithDetails(
        [FromQuery] PollStatusEnum? status,
        [FromQuery] ulong? userId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        [FromQuery] string sortBy = "totalVotes",
        [FromQuery] string sortOrder = "desc",
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/polls/with-details";

        _fitzMetrics?.RecordApiRequest(endpoint, "GET");

        try
        {
            _logger.LogInformation("Get polls with details request received. Status: {Status}, UserId: {UserId}, Skip: {Skip}, Take: {Take}", 
                status, userId, skip, take);

            var command = GetPollsWithDetailsCommand.From(status, userId, skip, take, sortBy, sortOrder);

            var response = await _getPollsWithDetailsFacade.Execute(command, cancellationToken);

            var dto = GetPollsWithDetailsResponseDto.From(response);

            _logger.LogInformation("Get polls with details completed successfully. TotalCount: {TotalCount}, Returned: {Returned}", 
                dto.TotalCount, dto.Polls.Count);

            return Ok(new ApiResponse<GetPollsWithDetailsResponseDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get polls with details failed - unexpected error");
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
