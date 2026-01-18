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
    public class AdminCreateLotteryController : ControllerBase
    {
        private readonly LotteryService _lotteryService;
        private readonly FitzMetrics? _fitzMetrics;

        public AdminCreateLotteryController(LotteryService lotteryService, FitzMetrics? fitzMetrics = null)
        {
            _lotteryService = lotteryService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpPost]
        [RequireDiscordAuth]
        [RequireAdmin]
        public async Task<IActionResult> CreateLottery([FromBody] AdminCreateLotteryRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/admin/lottery";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "POST");
            
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

                var startDate = request.StartDate ?? DateTime.UtcNow;
                var endDate = request.EndDate ?? DateTime.UtcNow.AddDays(7);

                await _lotteryService.StartNewLotteryAsync(startDate, endDate, request.Pool);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Lottery created successfully"
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
