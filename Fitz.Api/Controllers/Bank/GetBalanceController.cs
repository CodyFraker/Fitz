using Fitz.Api.Attributes;
using Fitz.Api.Models.Responses;
using Fitz.Features.Accounts;
using Fitz.Features.Bank;
using Microsoft.AspNetCore.Mvc;

namespace Fitz.Api.Controllers.Bank
{
    [ApiController]
    [Route("api/bank")]
    public class GetBalanceController : ControllerBase
    {
        private readonly BankService _bankService;
        private readonly AccountService _accountService;

        public GetBalanceController(BankService bankService, AccountService accountService)
        {
            _bankService = bankService;
            _accountService = accountService;
        }

        [HttpGet("balance/{userId}")]
        [RequireDiscordAuth]
        public IActionResult GetBalance(ulong userId)
        {
            var account = _accountService.FindAccount(userId);
            if (account == null)
            {
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
    }
}
