using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Rename.BuyoutRenames.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Rename.BuyoutRenames.Http;

[ApiController]
[Route("api/rename")]
public class BuyoutRenamesController(BuyoutRenamesFacade buyoutRenamesFacade, ILogger<BuyoutRenamesController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly BuyoutRenamesFacade _buyoutRenamesFacade = buyoutRenamesFacade;
    private readonly ILogger<BuyoutRenamesController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPost("user/{userId}/buyout")]
    [RequireDiscordAuth]
    public async Task<IActionResult> BuyoutRenames(ulong userId, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/rename/user/{userId}/buyout";

        _fitzMetrics?.RecordApiRequest(endpoint, "POST");

        try
        {
            _logger.LogInformation("Buyout renames request received. UserId: {UserId}", userId);

            var command = BuyoutRenamesCommand.From(userId);

            var response = await _buyoutRenamesFacade.Execute(command, cancellationToken);

            var dto = BuyoutRenamesResponseDto.From(response);

            _logger.LogInformation("Buyout renames completed successfully. UserId: {UserId}, UpdatedCount: {UpdatedCount}", userId, dto.RenamesUpdated);

            return Ok(new ApiResponse<BuyoutRenamesResponseDto>
            {
                Success = true,
                Message = response.Message,
                Data = dto
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Buyout renames failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "bad_request");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Buyout renames failed - unexpected error. UserId: {UserId}", userId);
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
