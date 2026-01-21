using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Admin.AdminSendMessage.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Admin.AdminSendMessage.Http;

[ApiController]
[Route("api/admin/bot")]
public class AdminSendMessageController(AdminSendMessageFacade adminSendMessageFacade, ILogger<AdminSendMessageController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly AdminSendMessageFacade _adminSendMessageFacade = adminSendMessageFacade;
    private readonly ILogger<AdminSendMessageController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPost("send-message")]
    [RequireDiscordAuth]
    [RequireAdmin]
    public async Task<IActionResult> SendMessage([FromBody] AdminSendMessageRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/admin/bot/send-message";

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

            _logger.LogInformation("Admin send message request received. ChannelId: {ChannelId}, MessageLength: {MessageLength}", 
                request.ChannelId, request.Message?.Length ?? 0);

            var command = request.ToCommand();

            var response = await _adminSendMessageFacade.Execute(command, cancellationToken);

            _logger.LogInformation("Admin send message completed successfully. MessageId: {MessageId}, ChannelId: {ChannelId}", 
                response.MessageId, response.ChannelId);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Message sent successfully",
                Data = new { MessageId = response.MessageId, ChannelId = response.ChannelId }
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Admin send message failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "validation_error");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError("Admin send message failed - invalid operation. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "not_found");

            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin send message failed - unexpected error");
            _fitzMetrics?.RecordApiError(endpoint, "exception");

            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = $"Failed to send message: {ex.Message}"
            });
        }
        finally
        {
            stopwatch.Stop();
            _fitzMetrics?.RecordApiRequestDuration(endpoint, stopwatch.Elapsed.TotalSeconds);
        }
    }
}
