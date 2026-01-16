using Fitz.Api.Attributes;
using Fitz.Api.Models.Requests;
using Fitz.Api.Models.Responses;
using Fitz.Features.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace Fitz.Api.Controllers.Account
{
    [ApiController]
    [Route("api/account")]
    public class SetLotterySubscribeController : ControllerBase
    {
        private readonly AccountService _accountService;

        public SetLotterySubscribeController(AccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("lottery-subscribe")]
        [RequireDiscordAuth]
        public async Task<IActionResult> SetLotterySubscribe([FromBody] SetLotterySubscribeRequest request)
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

            var result = await _accountService.SetLotterySubscribe(account, request.Subscribe);

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
