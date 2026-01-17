using Fitz.Api.Attributes;
using Fitz.Api.Models.Responses;
using Fitz.Features.Accounts;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Account
{
    [ApiController]
    [Route("api/account")]
    public class GetAccountController : ControllerBase
    {
        private readonly AccountService _accountService;
        private readonly FitzMetrics? _fitzMetrics;

        public GetAccountController(AccountService accountService, FitzMetrics? fitzMetrics = null)
        {
            _accountService = accountService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpGet("{userId}")]
        [RequireDiscordAuth]
        public IActionResult GetAccount(ulong userId)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/account";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                var account = _accountService.FindAccount(userId);
                if (account == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<AccountResponse>
                    {
                        Success = false,
                        Message = "Account not found"
                    });
                }

                var response = new AccountResponse
                {
                    Id = account.Id,
                    Username = account.Username,
                    Beer = account.Beer,
                    LifetimeBeer = account.LifetimeBeer,
                    SafeBalance = account.safeBalance,
                    Favorability = account.Favorability,
                    CreatedDate = account.CreatedDate,
                    SubscribeToLottery = account.subscribeToLottery,
                    SubscribeTickets = account.SubscribeTickets,
                    Deactivated = account.Deactivated
                };

                return Ok(new ApiResponse<AccountResponse>
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
