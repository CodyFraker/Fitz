using Fitz.Api.Attributes;
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
    public class GetPollVotesController : ControllerBase
    {
        private readonly PollService _pollService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FitzMetrics? _fitzMetrics;

        public GetPollVotesController(PollService pollService, IServiceScopeFactory scopeFactory, FitzMetrics? fitzMetrics = null)
        {
            _pollService = pollService;
            _scopeFactory = scopeFactory;
            _fitzMetrics = fitzMetrics;
        }

        [HttpGet("{id}/votes")]
        [RequireDiscordAuth]
        public async Task<IActionResult> GetPollVotes(int id)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/polls/{id}/votes";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                var poll = await db.Polls.FindAsync(id);
                if (poll == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<List<VoteResponse>>
                    {
                        Success = false,
                        Message = "Poll not found"
                    });
                }

                var votes = await db.Votes
                    .Where(v => v.PollId == id)
                    .ToListAsync();

                var response = votes.Select(v => new VoteResponse
                {
                    Id = v.Id,
                    PollId = v.PollId,
                    Choice = v.Choice,
                    UserId = v.UserId,
                    Timestamp = v.Timestamp
                }).ToList();

                return Ok(new ApiResponse<List<VoteResponse>>
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
