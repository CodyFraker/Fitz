using Fitz.Api.Attributes;
using Fitz.Api.Models.Requests;
using Fitz.Api.Models.Responses;
using Fitz.Core.Contexts;
using Fitz.Features.Polls;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Polls
{
    [ApiController]
    [Route("api/polls")]
    public class EvaluatePollController : ControllerBase
    {
        private readonly PollService _pollService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FitzMetrics? _fitzMetrics;

        public EvaluatePollController(PollService pollService, IServiceScopeFactory scopeFactory, FitzMetrics? fitzMetrics = null)
        {
            _pollService = pollService;
            _scopeFactory = scopeFactory;
            _fitzMetrics = fitzMetrics;
        }

        [HttpPatch("{id}/evaluate")]
        [RequireDiscordAuth]
        public async Task<IActionResult> EvaluatePoll(int id, [FromBody] EvaluatePollRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/polls/{id}/evaluate";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "PATCH");
            
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

                var result = await _pollService.EvaluatePoll(poll, request.Status);

                if (!result.Success)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "business_error");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = result.Message
                    });
                }

                var updatedPoll = result.Data as Fitz.Features.Polls.Models.Poll;
                if (updatedPoll == null)
                {
                    updatedPoll = await db.Polls.FindAsync(id);
                }

                if (updatedPoll == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<PollResponse>
                    {
                        Success = false,
                        Message = "Poll not found after evaluation"
                    });
                }

                var response = new PollResponse
                {
                    Id = updatedPoll.Id,
                    AccountId = updatedPoll.AccountId,
                    MessageId = updatedPoll.MessageId,
                    Question = updatedPoll.Question,
                    Type = updatedPoll.Type,
                    Status = updatedPoll.Status,
                    EvaluatedOn = updatedPoll.EvaluatedOn,
                    SubmittedOn = updatedPoll.SubmittedOn
                };

                return Ok(new ApiResponse<PollResponse>
                {
                    Success = true,
                    Message = result.Message,
                    Data = response
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
