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
    public class AddVoteController : ControllerBase
    {
        private readonly PollService _pollService;
        private readonly AccountService _accountService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FitzMetrics? _fitzMetrics;

        public AddVoteController(PollService pollService, AccountService accountService, IServiceScopeFactory scopeFactory, FitzMetrics? fitzMetrics = null)
        {
            _pollService = pollService;
            _accountService = accountService;
            _scopeFactory = scopeFactory;
            _fitzMetrics = fitzMetrics;
        }

        [HttpPost("{id}/vote")]
        [RequireDiscordAuth]
        public async Task<IActionResult> AddVote(int id, [FromBody] AddVoteRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/polls/{id}/vote";
            
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

                var option = await db.PollsOptions.FirstOrDefaultAsync(o => o.PollId == id && o.Id == request.OptionId);
                if (option == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Poll option not found"
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

                await _pollService.AddVote(poll, option, account);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Vote added successfully"
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
