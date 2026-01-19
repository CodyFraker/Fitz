using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Api.Attributes;
using Fitz.Api.Models.Responses;
using Fitz.Database;
using Fitz.Features.Polls;
using Fitz.Database.Entities;
using Fitz.Metrics;
using Fitz.Variables.Channels;
using Fitz.Variables.Emojis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Polls
{
    [ApiController]
    [Route("api/polls")]
    public class PostPollToPendingController : ControllerBase
    {
        private readonly DiscordClient _discordClient;
        private readonly PollService _pollService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FitzMetrics? _fitzMetrics;

        public PostPollToPendingController(DiscordClient discordClient, PollService pollService, IServiceScopeFactory scopeFactory, FitzMetrics? fitzMetrics = null)
        {
            _discordClient = discordClient;
            _pollService = pollService;
            _scopeFactory = scopeFactory;
            _fitzMetrics = fitzMetrics;
        }

        [HttpPost("{id}/post-to-pending")]
        [RequireDiscordAuth]
        public async Task<IActionResult> PostPollToPending(int id)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/polls/{id}/post-to-pending";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "POST");
            
            try
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                var poll = await db.Polls.FindAsync(id);
                if (poll == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<PollResponse>
                    {
                        Success = false,
                        Message = "Poll not found"
                    });
                }

                if (poll.MessageId != 0)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "already_posted");
                    return BadRequest(new ApiResponse<PollResponse>
                    {
                        Success = false,
                        Message = "Poll has already been posted to Discord"
                    });
                }

                if (poll.Status != PollStatusEnum.Pending)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "invalid_status");
                    return BadRequest(new ApiResponse<PollResponse>
                    {
                        Success = false,
                        Message = "Only pending polls can be posted to the pending channel"
                    });
                }

                var pollOptions = _pollService.GetPollOptions(poll);
                if (pollOptions == null || pollOptions.Count == 0)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "no_options");
                    return BadRequest(new ApiResponse<PollResponse>
                    {
                        Success = false,
                        Message = "Poll has no options"
                    });
                }

                var channel = await _discordClient.GetChannelAsync(Waterbear.PendingPolls);
                if (channel == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "channel_not_found");
                    return NotFound(new ApiResponse<PollResponse>
                    {
                        Success = false,
                        Message = "Pending polls channel not found"
                    });
                }

                var embed = _pollService.GeneratePollEmbed(_discordClient, poll, pollOptions);
                var pollMessage = await channel.SendMessageAsync(embed);

                try
                {
                    await pollMessage.CreateReactionAsync(DiscordEmoji.FromGuildEmote(_discordClient, PollEmojis.Yes));
                    await pollMessage.CreateReactionAsync(DiscordEmoji.FromGuildEmote(_discordClient, PollEmojis.No));
                }
                catch (Exception ex)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "reaction_error");
                    return StatusCode(500, new ApiResponse<PollResponse>
                    {
                        Success = false,
                        Message = $"Failed to add approval reactions: {ex.Message}"
                    });
                }

                poll.MessageId = pollMessage.Id;
                var updateResult = await _pollService.UpdatePollAsync(poll);
                if (!updateResult.Success)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "update_error");
                    return StatusCode(500, new ApiResponse<PollResponse>
                    {
                        Success = false,
                        Message = $"Failed to update poll MessageId: {updateResult.Message}"
                    });
                }

                var response = new PollResponse
                {
                    Id = poll.Id,
                    AccountId = poll.AccountId,
                    MessageId = poll.MessageId,
                    Question = poll.Question,
                    Type = poll.Type,
                    Status = poll.Status,
                    EvaluatedOn = poll.EvaluatedOn,
                    SubmittedOn = poll.SubmittedOn
                };

                return Ok(new ApiResponse<PollResponse>
                {
                    Success = true,
                    Message = "Poll posted to pending channel successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _fitzMetrics?.RecordApiError(endpoint, "exception");
                return StatusCode(500, new ApiResponse<PollResponse>
                {
                    Success = false,
                    Message = $"Failed to post poll to pending channel: {ex.Message}"
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
