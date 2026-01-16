using Fitz.Api.Attributes;
using Fitz.Api.Models.Responses;
using Fitz.Features.Lottery;
using Microsoft.AspNetCore.Mvc;

namespace Fitz.Api.Controllers.Lottery
{
    [ApiController]
    [Route("api/lottery")]
    public class GetCurrentLotteryController : ControllerBase
    {
        private readonly LotteryService _lotteryService;

        public GetCurrentLotteryController(LotteryService lotteryService)
        {
            _lotteryService = lotteryService;
        }

        [HttpGet("current")]
        [RequireDiscordAuth]
        public IActionResult GetCurrentLottery()
        {
            var lottery = _lotteryService.GetCurrentLottery();
            if (lottery == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "No active lottery found"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = lottery
            });
        }
    }
}
