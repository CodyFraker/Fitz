using Fitz.Api.Attributes;
using Fitz.Api.Models.Requests;
using Fitz.Api.Models.Responses;
using Fitz.Features.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace Fitz.Api.Controllers.Account
{
    [ApiController]
    [Route("api/account")]
    public class SetTicketAmountController : ControllerBase
    {
        private readonly AccountService _accountService;

        public SetTicketAmountController(AccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("ticket-amount")]
        [RequireDiscordAuth]
        public async Task<IActionResult> SetTicketAmount([FromBody] SetTicketAmountRequest request)
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

            var result = await _accountService.SetTicketAmountAsync(account, request.Amount);

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
