using Fitz.Api.Attributes;
using Fitz.Api.Models.Requests;
using Fitz.Api.Models.Responses;
using Fitz.Features.Lottery;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/lottery")]
    public class AdminModifyLotteryPoolController : ControllerBase
    {
        private readonly LotteryService _lotteryService;
        private readonly FitzMetrics? _fitzMetrics;

        public AdminModifyLotteryPoolController(LotteryService lotteryService, FitzMetrics? fitzMetrics = null)
        {
            _lotteryService = lotteryService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpPatch("current/pool")]
        [RequireDiscordAuth]
        [RequireAdmin]
        public async Task<IActionResult> ModifyLotteryPool([FromBody] AdminModifyLotteryPoolRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/admin/lottery/current/pool";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "PATCH");
            
            try
            {
                if (!ModelState.IsValid)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "validation_error");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request"
                    });
                }

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

                var result = await _lotteryService.SetLotteryPrizePoolAsync(request.Pool);
                if (!result.Success)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "operation_failed");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = result.Message
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = result.Message
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
