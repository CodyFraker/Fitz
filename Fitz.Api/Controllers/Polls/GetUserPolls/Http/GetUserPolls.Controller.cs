using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Polls.GetUserPolls.Domain;
using Fitz.Api.Extensions;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Polls.GetUserPolls.Http;

[ApiController]
[Route("api/polls")]
public class GetUserPollsController(GetUserPollsFacade getUserPollsFacade, ILogger<GetUserPollsController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly GetUserPollsFacade _getUserPollsFacade = getUserPollsFacade;
    private readonly ILogger<GetUserPollsController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpGet("my-polls")]
    [RequireDiscordAuth]
    public async Task<IActionResult> GetUserPolls(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/polls/my-polls";

        _fitzMetrics?.RecordApiRequest(endpoint, "GET");

        try
        {
            var userId = User.RequireDiscordUserId();

            _logger.LogInformation("Get user polls request received. UserId: {UserId}", userId);

            var command = GetUserPollsCommand.From(userId);

            var response = await _getUserPollsFacade.Execute(command, cancellationToken);

            var dto = GetUserPollsResponseDto.From(response);

            _logger.LogInformation("Get user polls completed successfully. UserId: {UserId}, Count: {Count}", userId, dto.Polls.Count);

            return Ok(new ApiResponse<GetUserPollsResponseDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get user polls failed - unexpected error");
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
