using Fitz.Api.Attributes;
using Fitz.Api.Models.Requests;
using Fitz.Api.Models.Responses;
using Fitz.Core.Contexts;
using Fitz.Features.Accounts;
using Fitz.Features.Polls;
using Fitz.Features.Polls.Models;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Polls
{
    [ApiController]
    [Route("api/polls")]
    public class CreatePollController : ControllerBase
    {
        private readonly PollService _pollService;
        private readonly AccountService _accountService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FitzMetrics? _fitzMetrics;

        public CreatePollController(PollService pollService, AccountService accountService, IServiceScopeFactory scopeFactory, FitzMetrics? fitzMetrics = null)
        {
            _pollService = pollService;
            _accountService = accountService;
            _scopeFactory = scopeFactory;
            _fitzMetrics = fitzMetrics;
        }

        [HttpPost]
        [RequireDiscordAuth]
        public async Task<IActionResult> CreatePoll([FromBody] CreatePollRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/polls";
            
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

                var account = _accountService.FindAccount(request.AccountId);
                if (account == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Account not found"
                    });
                }

                var poll = new Poll
                {
                    AccountId = request.AccountId,
                    MessageId = request.MessageId,
                    Question = request.Question,
                    Type = request.Type,
                    Status = PollStatus.Pending,
                    SubmittedOn = DateTime.UtcNow
                };

                var result = await _pollService.AddPoll(poll);

                if (!result.Success)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "business_error");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = result.Message
                    });
                }

                var savedPoll = result.Data as Poll;
                if (savedPoll == null)
                {
                    using var scope = _scopeFactory.CreateScope();
                    using var db = scope.ServiceProvider.GetRequiredService<BotContext>();
                    savedPoll = await db.Polls
                        .Where(p => p.MessageId == request.MessageId)
                        .OrderByDescending(p => p.SubmittedOn)
                        .FirstOrDefaultAsync();
                }

                if (savedPoll == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<PollResponse>
                    {
                        Success = false,
                        Message = "Poll created but could not be retrieved"
                    });
                }

                var pollOptions = request.Options.Select(o => new PollOptions
                {
                    PollId = savedPoll.Id,
                    Answer = o.Answer,
                    EmojiName = o.EmojiName,
                    EmojiId = o.EmojiId
                }).ToList();

                var addOptionsResult = await _pollService.AddPollOption(savedPoll, pollOptions);

                if (!addOptionsResult.Success)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "business_error");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = addOptionsResult.Message
                    });
                }

                var response = new PollResponse
                {
                    Id = savedPoll.Id,
                    AccountId = savedPoll.AccountId,
                    MessageId = savedPoll.MessageId,
                    Question = savedPoll.Question,
                    Type = savedPoll.Type,
                    Status = savedPoll.Status,
                    EvaluatedOn = savedPoll.EvaluatedOn,
                    SubmittedOn = savedPoll.SubmittedOn
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
