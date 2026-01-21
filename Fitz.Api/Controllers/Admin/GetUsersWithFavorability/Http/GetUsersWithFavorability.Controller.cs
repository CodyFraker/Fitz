using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Admin.GetUsersWithFavorability.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Admin.GetUsersWithFavorability.Http;

[ApiController]
[Route("api/admin/favorability")]
public class GetUsersWithFavorabilityController(GetUsersWithFavorabilityFacade getUsersWithFavorabilityFacade, ILogger<GetUsersWithFavorabilityController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly GetUsersWithFavorabilityFacade _getUsersWithFavorabilityFacade = getUsersWithFavorabilityFacade;
    private readonly ILogger<GetUsersWithFavorabilityController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpGet("users")]
    [RequireDiscordAuth]
    [RequireAdmin]
    public async Task<IActionResult> GetUsersWithFavorability(
        [FromQuery] string? query = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc",
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/admin/favorability/users";

        _fitzMetrics?.RecordApiRequest(endpoint, "GET");

        try
        {
            _logger.LogInformation("Get users with favorability request received. Query: {Query}, Skip: {Skip}, Take: {Take}, SortBy: {SortBy}, SortOrder: {SortOrder}", 
                query, skip, take, sortBy, sortOrder);

            var command = GetUsersWithFavorabilityCommand.From(query, skip, take, sortBy, sortOrder);

            var response = await _getUsersWithFavorabilityFacade.Execute(command, cancellationToken);

            var dto = GetUsersWithFavorabilityResponseDto.From(response);

            _logger.LogInformation("Get users with favorability completed successfully. TotalCount: {TotalCount}, Returned: {Returned}", 
                dto.TotalCount, dto.Users.Count);

            return Ok(new ApiResponse<GetUsersWithFavorabilityResponseDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Get users with favorability failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "validation_error");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get users with favorability failed - unexpected error");
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
