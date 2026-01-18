using Fitz.Api.Attributes;
using Fitz.Api.Models.Requests;
using Fitz.Api.Models.Responses;
using Fitz.Database;
using Fitz.Features.Accounts;
using Fitz.Features.Polls;
using Fitz.Database.Entities;
using Fitz.Features.Settings;
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
        private readonly SettingsService _settingsService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FitzMetrics? _fitzMetrics;

        public CreatePollController(PollService pollService, AccountService accountService, SettingsService settingsService, IServiceScopeFactory scopeFactory, FitzMetrics? fitzMetrics = null)
        {
            _pollService = pollService;
            _accountService = accountService;
            _settingsService = settingsService;
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

                var settings = _settingsService.GetSettings();

                if (account.Beer < (settings.PollSubmittedPenalty + settings.PollDeclinedPenalty))
                {
                    _fitzMetrics?.RecordApiError(endpoint, "insufficient_beer");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"You need at least {settings.PollSubmittedPenalty + settings.PollDeclinedPenalty} beer to create a poll."
                    });
                }

                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                var pendingPolls = await db.Polls
                    .Where(p => p.AccountId == request.AccountId && p.Status == PollStatus.Pending)
                    .CountAsync();

                if (pendingPolls >= settings.MaxPendingPolls)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "max_pending_polls");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"You have reached the maximum number of polls you can submit. You can have a maximum of {settings.MaxPendingPolls} polls submitted at a time."
                    });
                }

                switch (request.Type)
                {
                    case PollType.Number:
                        if (request.Options.Count < 2 || request.Options.Count > 10)
                        {
                            _fitzMetrics?.RecordApiError(endpoint, "invalid_option_count");
                            return BadRequest(new ApiResponse<object>
                            {
                                Success = false,
                                Message = "Number polls require between 2 and 10 options."
                            });
                        }
                        break;

                    case PollType.Color:
                        if (request.Options.Count < 1 || request.Options.Count > 9)
                        {
                            _fitzMetrics?.RecordApiError(endpoint, "invalid_option_count");
                            return BadRequest(new ApiResponse<object>
                            {
                                Success = false,
                                Message = "Color polls require between 1 and 9 options."
                            });
                        }
                        break;

                    case PollType.YesOrNo:
                        if (request.Options.Count != 2)
                        {
                            _fitzMetrics?.RecordApiError(endpoint, "invalid_option_count");
                            return BadRequest(new ApiResponse<object>
                            {
                                Success = false,
                                Message = "Yes/No polls require exactly 2 options (Yes and No)."
                            });
                        }
                        break;

                    case PollType.ThisOrThat:
                        if (request.Options.Count != 2)
                        {
                            _fitzMetrics?.RecordApiError(endpoint, "invalid_option_count");
                            return BadRequest(new ApiResponse<object>
                            {
                                Success = false,
                                Message = "This or That polls require exactly 2 options."
                            });
                        }
                        break;

                    case PollType.HotTake:
                        if (request.Options.Count != 2)
                        {
                            _fitzMetrics?.RecordApiError(endpoint, "invalid_option_count");
                            return BadRequest(new ApiResponse<object>
                            {
                                Success = false,
                                Message = "Hot Take polls require exactly 2 options (Agree and Shit Take)."
                            });
                        }
                        break;
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
                    using var dbContext = scope.ServiceProvider.GetRequiredService<BotContext>();
                    savedPoll = await dbContext.Polls
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
