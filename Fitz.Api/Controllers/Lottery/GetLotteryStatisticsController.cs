using Fitz.Api.Attributes;
using Fitz.Api.Models.Responses;
using Fitz.Features.Lottery;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Lottery
{
    [ApiController]
    [Route("api/lottery")]
    public class GetLotteryStatisticsController : ControllerBase
    {
        private readonly LotteryService _lotteryService;
        private readonly FitzMetrics? _fitzMetrics;

        public GetLotteryStatisticsController(LotteryService lotteryService, FitzMetrics? fitzMetrics = null)
        {
            _lotteryService = lotteryService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpGet("statistics")]
        [RequireDiscordAuth]
        public IActionResult GetLotteryStatistics()
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/lottery/statistics";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                var statistics = _lotteryService.GetLotteryStatistics();

                var dataPoints = statistics.Select(stat => new LotteryStatisticsPoint
                {
                    Date = stat.date,
                    PrizePool = stat.prizePool,
                    TotalTickets = stat.totalTickets
                }).ToList();

                var response = new LotteryStatisticsResponse
                {
                    DataPoints = dataPoints
                };

                return Ok(new ApiResponse<LotteryStatisticsResponse>
                {
                    Success = true,
                    Data = response
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
