using Fitz.Api.Attributes;
using Fitz.Api.Extensions;
using Fitz.Api.Models.Responses;
using Fitz.Features.Accounts;
using Fitz.Features.Bank;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Fitz.Api.Controllers.Bank
{
    [ApiController]
    [Route("api/bank")]
    public class GetBalanceController : ControllerBase
    {
        private readonly BankService _bankService;
        private readonly AccountService _accountService;
        private readonly FitzMetrics? _fitzMetrics;
        private readonly ILogger<GetBalanceController>? _logger;

        public GetBalanceController(BankService bankService, AccountService accountService, FitzMetrics? fitzMetrics = null, ILogger<GetBalanceController>? logger = null)
        {
            _bankService = bankService;
            _accountService = accountService;
            _fitzMetrics = fitzMetrics;
            _logger = logger;
        }

        [HttpGet("balance/{userId}")]
        [RequireDiscordAuth]
        [RequireOwnData]
        public async Task<IActionResult> GetBalance(ulong userId)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/bank/balance";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                var account = _accountService.FindAccount(userId);
                if (account == null)
                {
                    var username = User.GetDiscordUsername();
                    _logger?.LogInformation("Account not found for user {UserId} in balance request, creating new account with username {Username}", userId, username);
                    
                    var createResult = await _accountService.CreateAccountAsync(userId, username);
                    if (!createResult.Success)
                    {
                        _fitzMetrics?.RecordApiError(endpoint, "account_creation_failed");
                        _logger?.LogError("Failed to create account for user {UserId}", userId);
                        return StatusCode(500, new ApiResponse<BalanceResponse>
                        {
                            Success = false,
                            Message = "Failed to create account"
                        });
                    }

                    account = createResult.Data as Fitz.Database.Entities.Account;
                    if (account == null)
                    {
                        _fitzMetrics?.RecordApiError(endpoint, "account_creation_invalid");
                        return StatusCode(500, new ApiResponse<BalanceResponse>
                        {
                            Success = false,
                            Message = "Account creation returned invalid data"
                        });
                    }

                    _logger?.LogInformation("Successfully created account for user {UserId} in balance request", userId);
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
