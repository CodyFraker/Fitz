using Fitz.Api.Attributes;
using Fitz.Api.Models.Requests;
using Fitz.Api.Models.Responses;
using Fitz.Features.Accounts;
using Fitz.Features.Lottery;
using Microsoft.AspNetCore.Mvc;

namespace Fitz.Api.Controllers.Lottery
{
    [ApiController]
    [Route("api/lottery")]
    public class BuyTicketsController : ControllerBase
    {
        private readonly LotteryService _lotteryService;
        private readonly AccountService _accountService;

        public BuyTicketsController(LotteryService lotteryService, AccountService accountService)
        {
            _lotteryService = lotteryService;
            _accountService = accountService;
        }

        [HttpPost("buy-tickets")]
        [RequireDiscordAuth]
        public async Task<IActionResult> BuyTickets([FromBody] BuyTicketsRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request"
                });
            }

            var account = _accountService.FindAccount(request.UserId);
            if (account == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Account not found"
                });
            }

            var result = await _lotteryService.BuyTicketsForUser(account, request.Amount);

            if (!result.Success)
            {
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
    }
}
