using Fitz.Api.Attributes;
using Fitz.Api.Models.Requests;
using Fitz.Api.Models.Responses;
using Fitz.Features.Bank;
using Fitz.Features.Bank.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fitz.Api.Controllers.Bank
{
    [ApiController]
    [Route("api/bank")]
    public class AwardBonusController : ControllerBase
    {
        private readonly BankService _bankService;

        public AwardBonusController(BankService bankService)
        {
            _bankService = bankService;
        }

        [HttpPost("award-bonus")]
        [RequireDiscordAuth]
        public async Task<IActionResult> AwardBonus([FromBody] AwardBonusRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request"
                });
            }

            var result = await _bankService.AwardBonus(request.UserId, request.Amount);

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
