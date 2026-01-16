using Fitz.Api.Attributes;
using Fitz.Api.Models.Responses;
using Fitz.Features.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace Fitz.Api.Controllers.Account
{
    [ApiController]
    [Route("api/account")]
    public class GetAccountController : ControllerBase
    {
        private readonly AccountService _accountService;

        public GetAccountController(AccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet("{userId}")]
        [RequireDiscordAuth]
        public IActionResult GetAccount(ulong userId)
        {
            var account = _accountService.FindAccount(userId);
            if (account == null)
            {
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
    }
}
