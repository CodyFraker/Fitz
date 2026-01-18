using Fitz.Api.Attributes;
using Fitz.Api.Extensions;
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
    public class GetUserPollsController : ControllerBase
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FitzMetrics? _fitzMetrics;

        public GetUserPollsController(IServiceScopeFactory scopeFactory, FitzMetrics? fitzMetrics = null)
        {
            _scopeFactory = scopeFactory;
            _fitzMetrics = fitzMetrics;
        }

        [HttpGet("my-polls")]
        [RequireDiscordAuth]
        public async Task<IActionResult> GetUserPolls()
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/polls/my-polls";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                var userId = User.RequireDiscordUserId();

                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                var polls = await db.Polls
                    .Where(p => p.AccountId == userId)
                    .OrderByDescending(p => p.SubmittedOn)
                    .ToListAsync();

                var pollIds = polls.Select(p => p.Id).ToList();

                var options = await db.PollsOptions
                    .Where(o => pollIds.Contains(o.PollId))
                    .ToListAsync();

                var votes = await db.Votes
                    .Where(v => pollIds.Contains(v.PollId))
                    .ToListAsync();

                var totalVotesByPoll = votes
                    .GroupBy(v => v.PollId)
                    .ToDictionary(g => g.Key, g => g.Count());

                var optionVoteCountsByPoll = votes
                    .Where(v => v.Choice.HasValue)
                    .GroupBy(v => new { v.PollId, OptionId = v.Choice.Value })
                    .ToDictionary(g => (g.Key.PollId, g.Key.OptionId), g => g.Count());

                var pollResponses = polls.Select(p =>
                {
                    var pollOptions = options.Where(o => o.PollId == p.Id).ToList();
                    var pollVoteCounts = pollOptions.ToDictionary(
                        o => o.Id,
                        o => optionVoteCountsByPoll.TryGetValue((p.Id, o.Id), out var count) ? count : 0
                    );

                    return new PollResponse
                    {
                        Id = p.Id,
                        AccountId = p.AccountId,
                        MessageId = p.MessageId,
                        Question = p.Question,
                        Type = p.Type,
                        Status = p.Status,
                        EvaluatedOn = p.EvaluatedOn,
                        SubmittedOn = p.SubmittedOn,
                        Options = pollOptions.Select(o => new PollOptionResponse
                        {
                            Id = o.Id,
                            PollId = o.PollId,
                            Answer = o.Answer,
                            EmojiName = o.EmojiName,
                            EmojiId = o.EmojiId
                        }).ToList(),
                        TotalVotes = totalVotesByPoll.GetValueOrDefault(p.Id, 0),
                        OptionVoteCounts = pollVoteCounts
                    };
                }).ToList();

                return Ok(new ApiResponse<List<PollResponse>>
                {
                    Success = true,
                    Data = pollResponses
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
