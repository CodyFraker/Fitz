using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Polls.CreatePoll.Domain;
using Fitz.Api.Controllers.Polls.Exceptions;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Polls.CreatePoll.Http;

[ApiController]
[Route("api/polls")]
public class CreatePollController(CreatePollFacade createPollFacade, ILogger<CreatePollController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly CreatePollFacade _createPollFacade = createPollFacade;
    private readonly ILogger<CreatePollController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPost]
    [RequireDiscordAuth]
    public async Task<IActionResult> CreatePoll([FromBody] CreatePollRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/polls";

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

            _logger.LogInformation("Create poll request received. AccountId: {AccountId}, Type: {Type}", request.AccountId, request.Type);

            var command = request.ToCommand();

            var response = await _createPollFacade.Execute(command, cancellationToken);

            var dto = CreatePollResponseDto.From(response);

            _logger.LogInformation("Create poll completed successfully. PollId: {PollId}", dto.Id);

            return Ok(new ApiResponse<CreatePollResponseDto>
            {
                Success = true,
                Message = $"Poll #{dto.Id} created.",
                Data = dto
            });
        }
        catch (AccountNotFound ex)
        {
            _logger.LogWarning("Create poll failed - account not found. AccountId: {AccountId}", ex.UserId);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InsufficientBeerException ex)
        {
            _logger.LogWarning("Create poll failed - insufficient beer. Required: {Required}, Current: {Current}", 
                ex.RequiredAmount, ex.CurrentBalance);
            _fitzMetrics?.RecordApiError(endpoint, "insufficient_beer");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (MaxPendingPollsReachedException ex)
        {
            _logger.LogWarning("Create poll failed - max pending polls reached. Current: {Current}, Max: {Max}", 
                ex.CurrentCount, ex.MaxCount);
            _fitzMetrics?.RecordApiError(endpoint, "max_pending_polls");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidPollOptionCountException ex)
        {
            _logger.LogWarning("Create poll failed - invalid option count. PollType: {PollType}, Actual: {Actual}, Expected: {Min}-{Max}", 
                ex.PollType, ex.ActualCount, ex.MinCount, ex.MaxCount);
            _fitzMetrics?.RecordApiError(endpoint, "invalid_option_count");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Create poll failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "bad_request");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create poll failed - unexpected error. AccountId: {AccountId}", request.AccountId);
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
