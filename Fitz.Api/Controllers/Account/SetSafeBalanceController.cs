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
    public class SetSafeBalanceController : ControllerBase
    {
        private readonly AccountService _accountService;
        private readonly FitzMetrics? _fitzMetrics;

        public SetSafeBalanceController(AccountService accountService, FitzMetrics? fitzMetrics = null)
        {
            _accountService = accountService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpPost("safe-balance")]
        [RequireDiscordAuth]
        public async Task<IActionResult> SetSafeBalance([FromBody] SetSafeBalanceRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/account/safe-balance";
            
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

                var result = await _accountService.SetSafeBalanceAsync(request.UserId, request.SafeBalance);

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
