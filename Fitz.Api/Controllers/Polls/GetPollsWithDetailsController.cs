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
    public class GetPollsWithDetailsController : ControllerBase
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FitzMetrics? _fitzMetrics;

        public GetPollsWithDetailsController(IServiceScopeFactory scopeFactory, FitzMetrics? fitzMetrics = null)
        {
            _scopeFactory = scopeFactory;
            _fitzMetrics = fitzMetrics;
        }

        [HttpGet("with-details")]
        [RequireDiscordAuth]
        public async Task<IActionResult> GetPollsWithDetails(
            [FromQuery] PollStatusEnum? status,
            [FromQuery] ulong? userId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            [FromQuery] string sortBy = "totalVotes",
            [FromQuery] string sortOrder = "desc")
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/polls/with-details";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                var query = db.Polls.AsQueryable();

                if (status.HasValue)
                {
                    query = query.Where(p => p.Status == status.Value);
                }
                else
                {
                    query = query.Where(p => p.Status == PollStatusEnum.Approved);
                }

                if (userId.HasValue)
                {
                    query = query.Where(p => p.AccountId == userId.Value);
                }

                var polls = await query.ToListAsync();
                var pollIds = polls.Select(p => p.Id).ToList();

                var options = await db.PollsOptions
                    .Where(o => pollIds.Contains(o.PollId))
                    .ToListAsync();

                var votes = await db.Votes
                    .Where(v => pollIds.Contains(v.PollId))
                    .ToListAsync();

                var voteCountsByOption = votes
                    .Where(v => v.Choice.HasValue)
                    .GroupBy(v => v.Choice.Value)
                    .ToDictionary(g => g.Key, g => g.Count());

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

                switch (sortBy.ToLower())
                {
                    case "totalvotes":
                        pollResponses = sortOrder.ToLower() == "asc"
                            ? pollResponses.OrderBy(p => p.TotalVotes).ToList()
                            : pollResponses.OrderByDescending(p => p.TotalVotes).ToList();
                        break;
                    case "submittedon":
                        pollResponses = sortOrder.ToLower() == "asc"
                            ? pollResponses.OrderBy(p => p.SubmittedOn).ToList()
                            : pollResponses.OrderByDescending(p => p.SubmittedOn).ToList();
                        break;
                    case "question":
                        pollResponses = sortOrder.ToLower() == "asc"
                            ? pollResponses.OrderBy(p => p.Question).ToList()
                            : pollResponses.OrderByDescending(p => p.Question).ToList();
                        break;
                    default:
                        pollResponses = pollResponses.OrderByDescending(p => p.TotalVotes).ToList();
                        break;
                }

                var totalCount = pollResponses.Count;
                var paginatedPolls = pollResponses.Skip(skip).Take(take).ToList();

                var response = new PollsResponse
                {
                    Polls = paginatedPolls,
                    TotalCount = totalCount,
                    Skip = skip,
                    Take = take
                };

                return Ok(new ApiResponse<PollsResponse>
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
