using Fitz.Api.Attributes;
using Fitz.Api.Extensions;
using Fitz.Api.Models.Responses;
using Fitz.Features.Accounts;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Fitz.Api.Controllers.Account
{
    [ApiController]
    [Route("api/account")]
    public class GetAccountController : ControllerBase
    {
        private readonly AccountService _accountService;
        private readonly FitzMetrics? _fitzMetrics;
        private readonly ILogger<GetAccountController>? _logger;

        public GetAccountController(AccountService accountService, FitzMetrics? fitzMetrics = null, ILogger<GetAccountController>? logger = null)
        {
            _accountService = accountService;
            _fitzMetrics = fitzMetrics;
            _logger = logger;
        }

        [HttpGet("{userId}")]
        [RequireDiscordAuth]
        [RequireOwnData]
        public async Task<IActionResult> GetAccount(ulong userId)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/account";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                var account = _accountService.FindAccount(userId);
                if (account == null)
                {
                    var username = User.GetDiscordUsername();
                    _logger?.LogInformation("Account not found for user {UserId}, creating new account with username {Username}", userId, username);
                    
                    var createResult = await _accountService.CreateAccountAsync(userId, username);
                    if (!createResult.Success)
                    {
                        _fitzMetrics?.RecordApiError(endpoint, "account_creation_failed");
                        _logger?.LogError("Failed to create account for user {UserId}", userId);
                        return StatusCode(500, new ApiResponse<AccountResponse>
                        {
                            Success = false,
                            Message = "Failed to create account"
                        });
                    }

                    account = createResult.Data as Fitz.Database.Entities.Account;
                    if (account == null)
                    {
                        _fitzMetrics?.RecordApiError(endpoint, "account_creation_invalid");
                        return StatusCode(500, new ApiResponse<AccountResponse>
                        {
                            Success = false,
                            Message = "Account creation returned invalid data"
                        });
                    }

                    _logger?.LogInformation("Successfully created account for user {UserId}", userId);
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
