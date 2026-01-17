using Fitz.Api.Attributes;
using Fitz.Api.Models.Requests;
using Fitz.Api.Models.Responses;
using Fitz.Features.Accounts;
using Fitz.Features.Lottery;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Lottery
{
    [ApiController]
    [Route("api/lottery")]
    public class BuyTicketsController : ControllerBase
    {
        private readonly LotteryService _lotteryService;
        private readonly AccountService _accountService;
        private readonly FitzMetrics? _fitzMetrics;

        public BuyTicketsController(LotteryService lotteryService, AccountService accountService, FitzMetrics? fitzMetrics = null)
        {
            _lotteryService = lotteryService;
            _accountService = accountService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpPost("buy-tickets")]
        [RequireDiscordAuth]
        public async Task<IActionResult> BuyTickets([FromBody] BuyTicketsRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/lottery/buy-tickets";
            
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

                var result = await _lotteryService.BuyTicketsForUser(account, request.Amount);

                if (!result.Success)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "business_error");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = result.Message
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data
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
