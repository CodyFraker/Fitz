using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Rename.Exceptions;
using Fitz.Api.Controllers.Rename.GetRename.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Rename.GetRename.Http;

[ApiController]
[Route("api/rename")]
public class GetRenameController(GetRenameFacade getRenameFacade, ILogger<GetRenameController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly GetRenameFacade _getRenameFacade = getRenameFacade;
    private readonly ILogger<GetRenameController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpGet("{id}")]
    [RequireDiscordAuth]
    public async Task<IActionResult> GetRename(int id, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/rename/{id}";

        _fitzMetrics?.RecordApiRequest(endpoint, "GET");

        try
        {
            _logger.LogInformation("Get rename request received. Id: {Id}", id);

            var command = GetRenameCommand.From(id);

            var response = await _getRenameFacade.Execute(command, cancellationToken);

            var dto = GetRenameResponseDto.From(response);

            _logger.LogInformation("Get rename completed successfully. Id: {Id}", id);

            return Ok(new ApiResponse<GetRenameResponseDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (RenameNotFound ex)
        {
            _logger.LogWarning("Get rename failed - rename not found. Id: {Id}", ex.RenameId);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Get rename failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "bad_request");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get rename failed - unexpected error. Id: {Id}", id);
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
