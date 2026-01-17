using Fitz.Api.Attributes;
using Fitz.Api.Models.Requests;
using Fitz.Api.Models.Responses;
using Fitz.Features.Accounts;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Account
{
    [ApiController]
    [Route("api/account")]
    public class SetTicketAmountController : ControllerBase
    {
        private readonly AccountService _accountService;
        private readonly FitzMetrics? _fitzMetrics;

        public SetTicketAmountController(AccountService accountService, FitzMetrics? fitzMetrics = null)
        {
            _accountService = accountService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpPost("ticket-amount")]
        [RequireDiscordAuth]
        public async Task<IActionResult> SetTicketAmount([FromBody] SetTicketAmountRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/account/ticket-amount";
            
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

                var result = await _accountService.SetTicketAmountAsync(account, request.Amount);

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
