using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Rename.GetRenames.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Database.Entities;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Rename.GetRenames.Http;

[ApiController]
[Route("api/rename")]
public class GetRenamesController(GetRenamesFacade getRenamesFacade, ILogger<GetRenamesController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly GetRenamesFacade _getRenamesFacade = getRenamesFacade;
    private readonly ILogger<GetRenamesController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpGet]
    [RequireDiscordAuth]
    public async Task<IActionResult> GetRenames([FromQuery] RenameStatusEnum? status, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/rename";

        _fitzMetrics?.RecordApiRequest(endpoint, "GET");

        try
        {
            _logger.LogInformation("Get renames request received. Status: {Status}", status);

            var command = GetRenamesCommand.From(status);

            var response = await _getRenamesFacade.Execute(command, cancellationToken);

            var dto = GetRenamesResponseDto.From(response);

            _logger.LogInformation("Get renames completed successfully. Count: {Count}", dto.Renames.Count);

            return Ok(new ApiResponse<GetRenamesResponseDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get renames failed - unexpected error");
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
