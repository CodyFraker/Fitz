using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Polls.Exceptions;
using Fitz.Api.Controllers.Rename.CreateRename.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Rename.CreateRename.Http;

[ApiController]
[Route("api/rename")]
public class CreateRenameController(CreateRenameFacade createRenameFacade, ILogger<CreateRenameController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly CreateRenameFacade _createRenameFacade = createRenameFacade;
    private readonly ILogger<CreateRenameController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPost]
    [RequireDiscordAuth]
    public async Task<IActionResult> CreateRename([FromBody] CreateRenameRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/rename";

        _fitzMetrics?.RecordApiRequest(endpoint, "POST");

        try
        {
            if (!ModelState.IsValid)
            {
                _fitzMetrics?.RecordApiError(endpoint, "validation_error");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request"
                });
            }

            _logger.LogInformation("Create rename request received. AffectedUserId: {AffectedUserId}, RequestedUserId: {RequestedUserId}, NewName: {NewName}", 
                request.AffectedUserId, request.RequestedUserId, request.NewName);

            var command = request.ToCommand();

            var response = await _createRenameFacade.Execute(command, cancellationToken);

            var dto = CreateRenameResponseDto.From(response);

            _logger.LogInformation("Create rename completed successfully. RenameId: {RenameId}", dto.Id);

            return Ok(new ApiResponse<CreateRenameResponseDto>
            {
                Success = true,
                Message = "Rename created successfully",
                Data = dto
            });
        }
        catch (AccountNotFound ex)
        {
            _logger.LogWarning("Create rename failed - account not found. UserId: {UserId}", ex.UserId);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InsufficientBeerException ex)
        {
            _logger.LogWarning("Create rename failed - insufficient beer. Required: {Required}, Current: {Current}", 
                ex.RequiredAmount, ex.CurrentBalance);
            _fitzMetrics?.RecordApiError(endpoint, "insufficient_beer");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create rename failed - unexpected error. AffectedUserId: {AffectedUserId}, RequestedUserId: {RequestedUserId}", 
                request.AffectedUserId, request.RequestedUserId);
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
