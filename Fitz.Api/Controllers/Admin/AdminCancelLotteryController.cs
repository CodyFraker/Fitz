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
    public class AdminCancelLotteryController : ControllerBase
    {
        private readonly LotteryService _lotteryService;
        private readonly FitzMetrics? _fitzMetrics;

        public AdminCancelLotteryController(LotteryService lotteryService, FitzMetrics? fitzMetrics = null)
        {
            _lotteryService = lotteryService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpDelete("current")]
        [RequireDiscordAuth]
        [RequireAdmin]
        public async Task<IActionResult> CancelCurrentLottery()
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/admin/lottery/current";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "DELETE");
            
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

                await _lotteryService.EndLotteryAsync(currentLottery);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Lottery cancelled successfully"
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
