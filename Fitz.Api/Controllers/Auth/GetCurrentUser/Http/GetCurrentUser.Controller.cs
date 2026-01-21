using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Auth.GetCurrentUser.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Auth.GetCurrentUser.Http;

[ApiController]
[Route("api/auth")]
public class GetCurrentUserController(GetCurrentUserFacade getCurrentUserFacade, ILogger<GetCurrentUserController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly GetCurrentUserFacade _getCurrentUserFacade = getCurrentUserFacade;
    private readonly ILogger<GetCurrentUserController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpGet("me")]
    [RequireDiscordAuth]
    public IActionResult GetCurrentUser()
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/auth/me";

        _fitzMetrics?.RecordApiRequest(endpoint, "GET");

        try
        {
            _logger.LogInformation("Get current user request received");

            var command = GetCurrentUserCommand.From(User);

            var response = _getCurrentUserFacade.Execute(command);

            var dto = GetCurrentUserResponseDto.From(response);
            var currentUserResponse = dto.ToCurrentUserResponse();

            _logger.LogInformation("Get current user completed successfully. UserId: {UserId}", response.Id);

            return Ok(new ApiResponse<CurrentUserResponse>
            {
                Success = true,
                Data = currentUserResponse
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Get current user failed - unauthorized. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "unauthorized");

            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get current user failed - unexpected error");
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
