using DSharpPlus;
using Fitz.Api.Attributes;
using Fitz.Api.Models.Requests;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/bot")]
    public class AdminSendMessageController : ControllerBase
    {
        private readonly DiscordClient _discordClient;
        private readonly FitzMetrics? _fitzMetrics;

        public AdminSendMessageController(DiscordClient discordClient, FitzMetrics? fitzMetrics = null)
        {
            _discordClient = discordClient;
            _fitzMetrics = fitzMetrics;
        }

        [HttpPost("send-message")]
        [RequireDiscordAuth]
        [RequireAdmin]
        public async Task<IActionResult> SendMessage([FromBody] AdminSendMessageRequest request)
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

                var channel = await _discordClient.GetChannelAsync(request.ChannelId);
                if (channel == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Channel not found"
                    });
                }

                var message = await channel.SendMessageAsync(request.Message);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Message sent successfully",
                    Data = new { MessageId = message.Id, ChannelId = channel.Id }
                });
            }
            catch (Exception ex)
            {
                _fitzMetrics?.RecordApiError(endpoint, "exception");
                return StatusCode(500, new ApiResponse<object>
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
}
