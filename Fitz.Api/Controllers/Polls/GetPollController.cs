using Fitz.Api.Attributes;
using Fitz.Api.Models.Responses;
using Fitz.Database;
using Fitz.Database.Entities;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Polls
{
    [ApiController]
    [Route("api/polls")]
    public class GetPollController : ControllerBase
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FitzMetrics? _fitzMetrics;

        public GetPollController(IServiceScopeFactory scopeFactory, FitzMetrics? fitzMetrics = null)
        {
            _scopeFactory = scopeFactory;
            _fitzMetrics = fitzMetrics;
        }

        [HttpGet("{id}")]
        [RequireDiscordAuth]
        public IActionResult GetPoll(int id)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/polls/{id}";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                var poll = db.Polls.Find(id);
                if (poll == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<PollResponse>
                    {
                        Success = false,
                        Message = "Poll not found"
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
                    Data = response
                });
            }
            finally
            {
                stopwatch.Stop();
                _fitzMetrics?.RecordApiRequestDuration(endpoint, stopwatch.Elapsed.TotalSeconds);
            }
        }

        [HttpGet("message/{messageId}")]
        [RequireDiscordAuth]
        public IActionResult GetPollByMessageId(ulong messageId)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/polls/message/{messageId}";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                var poll = db.Polls.FirstOrDefault(p => p.MessageId == messageId);
                if (poll == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<PollResponse>
                    {
                        Success = false,
                        Message = "Poll not found"
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
