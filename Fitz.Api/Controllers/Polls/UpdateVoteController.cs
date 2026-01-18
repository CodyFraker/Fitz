using Fitz.Api.Attributes;
using Fitz.Api.Models.Requests;
using Fitz.Api.Models.Responses;
using Fitz.Database;
using Fitz.Features.Accounts;
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
    public class UpdateVoteController : ControllerBase
    {
        private readonly PollService _pollService;
        private readonly AccountService _accountService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FitzMetrics? _fitzMetrics;

        public UpdateVoteController(PollService pollService, AccountService accountService, IServiceScopeFactory scopeFactory, FitzMetrics? fitzMetrics = null)
        {
            _pollService = pollService;
            _accountService = accountService;
            _scopeFactory = scopeFactory;
            _fitzMetrics = fitzMetrics;
        }

        [HttpPut("{id}/vote")]
        [RequireDiscordAuth]
        public async Task<IActionResult> UpdateVote(int id, [FromBody] UpdateVoteRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/polls/{id}/vote";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "PUT");
            
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
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Poll not found"
                    });
                }

                var vote = await db.Votes
                    .FirstOrDefaultAsync(v => v.PollId == id && v.UserId == request.UserId);

                if (vote == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Vote not found"
                    });
                }

                var account = _accountService.FindAccount(request.UserId);
                if (account == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Account not found"
                    });
                }

                await _pollService.UpdateVote(vote, request.OptionId, account);

                var updatedVote = await db.Votes.FindAsync(vote.Id);
                var response = new VoteResponse
                {
                    Id = updatedVote.Id,
                    PollId = updatedVote.PollId,
                    Choice = updatedVote.Choice,
                    UserId = updatedVote.UserId,
                    Timestamp = updatedVote.Timestamp
                };

                return Ok(new ApiResponse<VoteResponse>
                {
                    Success = true,
                    Message = "Vote updated successfully",
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
