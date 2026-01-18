using Fitz.Api.Attributes;
using Fitz.Api.Models.Responses;
using Fitz.Features.Lottery;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/lottery")]
    public class AdminEndLotteryController : ControllerBase
    {
        private readonly LotteryService _lotteryService;
        private readonly FitzMetrics? _fitzMetrics;

        public AdminEndLotteryController(LotteryService lotteryService, FitzMetrics? fitzMetrics = null)
        {
            _lotteryService = lotteryService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpPost("current/end")]
        [RequireDiscordAuth]
        [RequireAdmin]
        public async Task<IActionResult> EndLottery()
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/admin/lottery/current/end";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "POST");
            
            try
            {
                var currentLottery = _lotteryService.GetCurrentLottery();
                if (currentLottery == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "No active lottery found"
                    });
                }

                await _lotteryService.EndLotteryAndDecideWinnersAsync(currentLottery);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Lottery ended successfully and winners have been determined"
                });
            }
            finally
            {
                stopwatch.Stop();
                _fitzMetrics?.RecordApiRequestDuration(endpoint, stopwatch.Elapsed.TotalSeconds);
            }
        }
    }
}
