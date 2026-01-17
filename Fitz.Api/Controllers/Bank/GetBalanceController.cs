using Fitz.Api.Attributes;
using Fitz.Api.Models.Responses;
using Fitz.Features.Accounts;
using Fitz.Features.Bank;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Bank
{
    [ApiController]
    [Route("api/bank")]
    public class GetBalanceController : ControllerBase
    {
        private readonly BankService _bankService;
        private readonly AccountService _accountService;
        private readonly FitzMetrics? _fitzMetrics;

        public GetBalanceController(BankService bankService, AccountService accountService, FitzMetrics? fitzMetrics = null)
        {
            _bankService = bankService;
            _accountService = accountService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpGet("balance/{userId}")]
        [RequireDiscordAuth]
        public IActionResult GetBalance(ulong userId)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/bank/balance";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                var account = _accountService.FindAccount(userId);
                if (account == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<BalanceResponse>
                    {
                        Success = false,
                        Message = "Account not found"
                    });
                }

                var response = new BalanceResponse
                {
                    Beer = account.Beer,
                    LifetimeBeer = account.LifetimeBeer
                };

                return Ok(new ApiResponse<BalanceResponse>
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
