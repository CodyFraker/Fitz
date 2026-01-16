using Fitz.Api.Attributes;
using Fitz.Api.Models.Requests;
using Fitz.Api.Models.Responses;
using Fitz.Features.Bank;
using Fitz.Features.Bank.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Fitz.Api.Controllers.Bank
{
    [ApiController]
    [Route("api/bank")]
    public class DeductBeerController : ControllerBase
    {
        private readonly BankService _bankService;

        public DeductBeerController(BankService bankService)
        {
            _bankService = bankService;
        }

        [HttpPost("deduct-beer")]
        [RequireDiscordAuth]
        public async Task<IActionResult> DeductBeer([FromBody] DeductBeerRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request"
                });
            }

            if (!Enum.TryParse<Reason>(request.Reason, out var reason))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid reason"
                });
            }

            var result = await _bankService.DeductBeerFromUser(request.UserId, request.Amount, reason);

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
