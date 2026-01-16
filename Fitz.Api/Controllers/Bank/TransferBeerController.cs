using Fitz.Api.Attributes;
using Fitz.Api.Models.Requests;
using Fitz.Api.Models.Responses;
using Fitz.Features.Bank;
using Microsoft.AspNetCore.Mvc;

namespace Fitz.Api.Controllers.Bank
{
    [ApiController]
    [Route("api/bank")]
    public class TransferBeerController : ControllerBase
    {
        private readonly BankService _bankService;

        public TransferBeerController(BankService bankService)
        {
            _bankService = bankService;
        }

        [HttpPost("transfer")]
        [RequireDiscordAuth]
        public async Task<IActionResult> TransferBeer([FromBody] TransferBeerRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request"
                });
            }

            var result = await _bankService.TransferBeer(request.SenderId, request.RecipientId, request.Amount);

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
