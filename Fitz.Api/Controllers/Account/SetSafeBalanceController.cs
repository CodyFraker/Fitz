using Fitz.Api.Attributes;
using Fitz.Api.Models.Requests;
using Fitz.Api.Models.Responses;
using Fitz.Features.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace Fitz.Api.Controllers.Account
{
    [ApiController]
    [Route("api/account")]
    public class SetSafeBalanceController : ControllerBase
    {
        private readonly AccountService _accountService;

        public SetSafeBalanceController(AccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("safe-balance")]
        [RequireDiscordAuth]
        public async Task<IActionResult> SetSafeBalance([FromBody] SetSafeBalanceRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request"
                });
            }

            var result = await _accountService.SetSafeBalanceAsync(request.UserId, request.SafeBalance);

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
