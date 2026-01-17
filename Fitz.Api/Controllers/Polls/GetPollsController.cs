using Fitz.Api.Attributes;
using Fitz.Api.Models.Responses;
using Fitz.Core.Contexts;
using Fitz.Features.Polls;
using Fitz.Features.Polls.Models;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Polls
{
    [ApiController]
    [Route("api/polls")]
    public class GetPollsController : ControllerBase
    {
        private readonly PollService _pollService;
        private readonly FitzMetrics? _fitzMetrics;

        public GetPollsController(PollService pollService, FitzMetrics? fitzMetrics = null)
        {
            _pollService = pollService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpGet]
        [RequireDiscordAuth]
        public IActionResult GetPolls([FromQuery] PollStatus? status, [FromQuery] ulong? userId)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/polls";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                List<Poll> polls;

                if (userId.HasValue)
                {
                    polls = _pollService.GetPollsSubmittedByUser(userId.Value);
                    if (status.HasValue)
                    {
                        polls = polls.Where(p => p.Status == status.Value).ToList();
                    }
                }
                else
                {
                    polls = _pollService.GetPolls();
                    if (status.HasValue)
                    {
                        polls = polls.Where(p => p.Status == status.Value).ToList();
                    }
                }

                var response = polls.Select(p => new PollResponse
                {
                    Id = p.Id,
                    AccountId = p.AccountId,
                    MessageId = p.MessageId,
                    Question = p.Question,
                    Type = p.Type,
                    Status = p.Status,
                    EvaluatedOn = p.EvaluatedOn,
                    SubmittedOn = p.SubmittedOn
                }).ToList();

                return Ok(new ApiResponse<List<PollResponse>>
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
