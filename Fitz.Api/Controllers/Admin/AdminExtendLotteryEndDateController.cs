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
    public class AdminExtendLotteryEndDateController : ControllerBase
    {
        private readonly LotteryService _lotteryService;
        private readonly FitzMetrics? _fitzMetrics;

        public AdminExtendLotteryEndDateController(LotteryService lotteryService, FitzMetrics? fitzMetrics = null)
        {
            _lotteryService = lotteryService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpPatch("current/end-date")]
        [RequireDiscordAuth]
        [RequireAdmin]
        public async Task<IActionResult> ExtendLotteryEndDate([FromBody] AdminExtendLotteryEndDateRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/admin/lottery/current/end-date";
            
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

                await _lotteryService.UpdateCurrentLottery(request.EndDate);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Lottery end date extended successfully"
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
