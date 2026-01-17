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
    public class GetPollOptionsController : ControllerBase
    {
        private readonly PollService _pollService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FitzMetrics? _fitzMetrics;

        public GetPollOptionsController(PollService pollService, IServiceScopeFactory scopeFactory, FitzMetrics? fitzMetrics = null)
        {
            _pollService = pollService;
            _scopeFactory = scopeFactory;
            _fitzMetrics = fitzMetrics;
        }

        [HttpGet("{id}/options")]
        [RequireDiscordAuth]
        public async Task<IActionResult> GetPollOptions(int id)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/polls/{id}/options";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                var poll = await db.Polls.FindAsync(id);
                if (poll == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<List<PollOptionResponse>>
                    {
                        Success = false,
                        Message = "Poll not found"
                    });
                }

                var options = _pollService.GetPollOptions(poll);

                var response = options.Select(o => new PollOptionResponse
                {
                    Id = o.Id,
                    PollId = o.PollId,
                    Answer = o.Answer,
                    EmojiName = o.EmojiName,
                    EmojiId = o.EmojiId
                }).ToList();

                return Ok(new ApiResponse<List<PollOptionResponse>>
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
