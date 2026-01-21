using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Users.GetUsers.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Users.GetUsers.Http;

[ApiController]
[Route("api/users")]
public class GetUsersController(GetUsersFacade getUsersFacade, ILogger<GetUsersController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly GetUsersFacade _getUsersFacade = getUsersFacade;
    private readonly ILogger<GetUsersController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpGet]
    [RequireDiscordAuth]
    public async Task<IActionResult> GetUsers([FromQuery] string? query = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/users";

        _fitzMetrics?.RecordApiRequest(endpoint, "GET");

        try
        {
            _logger.LogInformation("Get users request received. Query: {Query}, Page: {Page}, PageSize: {PageSize}", query, page, pageSize);

            var command = GetUsersCommand.From(query, page, pageSize);

            var response = await _getUsersFacade.Execute(command, cancellationToken);

            var dto = GetUsersResponseDto.From(response);

            _logger.LogInformation("Get users completed successfully. TotalCount: {TotalCount}, Page: {Page}, PageSize: {PageSize}", 
                dto.TotalCount, dto.Page, dto.PageSize);

            return Ok(new ApiResponse<GetUsersResponseDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Get users failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "validation_error");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get users failed - unexpected error");
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
