using Fitz.Api.Attributes;
using Fitz.Api.Extensions;
using Fitz.Api.Models.Requests;
using Fitz.Api.Models.Responses;
using Fitz.Database;
using Fitz.Features.Accounts;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/accounts")]
    public class AdminModifyAccountController : ControllerBase
    {
        private readonly AccountService _accountService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FitzMetrics? _fitzMetrics;

        public AdminModifyAccountController(AccountService accountService, IServiceScopeFactory scopeFactory, FitzMetrics? fitzMetrics = null)
        {
            _accountService = accountService;
            _scopeFactory = scopeFactory;
            _fitzMetrics = fitzMetrics;
        }

        [HttpPatch("{userId}")]
        [RequireDiscordAuth]
        [RequireAdmin]
        public async Task<IActionResult> ModifyAccount(ulong userId, [FromBody] AdminModifyAccountRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/admin/accounts/{userId}";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "PATCH");
            
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

                if (request.UserId != userId)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "validation_error");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "UserId in path must match UserId in request body"
                    });
                }

                var account = _accountService.FindAccount(userId);
                if (account == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Account not found"
                    });
                }

                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                if (request.Beer.HasValue)
                {
                    account.Beer = request.Beer.Value;
                    db.Update(account);
                    await db.SaveChangesAsync();
                }

                if (request.LifetimeBeer.HasValue)
                {
                    account.LifetimeBeer = request.LifetimeBeer.Value;
                    db.Update(account);
                    await db.SaveChangesAsync();
                }

                if (request.SafeBalance.HasValue)
                {
                    var result = await _accountService.SetSafeBalanceAsync(account, request.SafeBalance.Value);
                    if (!result.Success)
                    {
                        _fitzMetrics?.RecordApiError(endpoint, "business_error");
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            Message = result.Message
                        });
                    }
                }

                if (request.Favorability.HasValue)
                {
                    var result = await _accountService.SetFavorabilityAsync(account, request.Favorability.Value);
                    if (!result.Success)
                    {
                        _fitzMetrics?.RecordApiError(endpoint, "business_error");
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            Message = result.Message
                        });
                    }
                }

                if (request.SubscribeToLottery.HasValue)
                {
                    var result = await _accountService.SetLotterySubscribe(account, request.SubscribeToLottery.Value);
                    if (!result.Success)
                    {
                        _fitzMetrics?.RecordApiError(endpoint, "business_error");
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            Message = result.Message
                        });
                    }
                }

                if (request.SubscribeTickets.HasValue)
                {
                    var result = await _accountService.SetTicketAmountAsync(account, request.SubscribeTickets.Value);
                    if (!result.Success)
                    {
                        _fitzMetrics?.RecordApiError(endpoint, "business_error");
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            Message = result.Message
                        });
                    }
                }

                if (request.Deactivated.HasValue)
                {
                    var result = await _accountService.SetDeactivatedAsync(account, request.Deactivated.Value);
                    if (!result.Success)
                    {
                        _fitzMetrics?.RecordApiError(endpoint, "business_error");
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            Message = result.Message
                        });
                    }
                }

                var updatedAccount = _accountService.FindAccount(userId);
                var response = new AccountResponse
                {
                    Id = updatedAccount.Id,
                    Username = updatedAccount.Username,
                    Beer = updatedAccount.Beer,
                    LifetimeBeer = updatedAccount.LifetimeBeer,
                    SafeBalance = updatedAccount.safeBalance,
                    Favorability = updatedAccount.Favorability,
                    CreatedDate = updatedAccount.CreatedDate,
                    SubscribeToLottery = updatedAccount.subscribeToLottery,
                    SubscribeTickets = updatedAccount.SubscribeTickets,
                    Deactivated = updatedAccount.Deactivated
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
