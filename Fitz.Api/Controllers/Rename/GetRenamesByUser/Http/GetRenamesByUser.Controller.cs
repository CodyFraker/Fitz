using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Rename.GetRenamesByUser.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Rename.GetRenamesByUser.Http;

[ApiController]
[Route("api/rename")]
public class GetRenamesByUserController(GetRenamesByUserFacade getRenamesByUserFacade, ILogger<GetRenamesByUserController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly GetRenamesByUserFacade _getRenamesByUserFacade = getRenamesByUserFacade;
    private readonly ILogger<GetRenamesByUserController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpGet("user/{userId}")]
    [RequireDiscordAuth]
    [RequireOwnData]
    public async Task<IActionResult> GetRenamesByUser(ulong userId, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/rename/user/{userId}";

        _fitzMetrics?.RecordApiRequest(endpoint, "GET");

        try
        {
            _logger.LogInformation("Get renames by user request received. UserId: {UserId}", userId);

            var command = GetRenamesByUserCommand.From(userId);

            var response = await _getRenamesByUserFacade.Execute(command, cancellationToken);

            var dto = GetRenamesByUserResponseDto.From(response);

            _logger.LogInformation("Get renames by user completed successfully. UserId: {UserId}, Count: {Count}", userId, dto.Renames.Count);

            return Ok(new ApiResponse<GetRenamesByUserResponseDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get renames by user failed - unexpected error. UserId: {UserId}", userId);
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
