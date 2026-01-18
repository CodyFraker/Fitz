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
    public class GetCurrentLotteryController : ControllerBase
    {
        private readonly LotteryService _lotteryService;
        private readonly FitzMetrics? _fitzMetrics;

        public GetCurrentLotteryController(LotteryService lotteryService, FitzMetrics? fitzMetrics = null)
        {
            _lotteryService = lotteryService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpGet("current")]
        [RequireDiscordAuth]
        public IActionResult GetCurrentLottery()
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/lottery/current";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                var lottery = _lotteryService.GetCurrentLottery();
                if (lottery == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "No active lottery found"
                    });
                }

                var totalTicketsResult = _lotteryService.GetTotalTickets();
                var totalParticipantsResult = _lotteryService.GetTotalLotteryParticipant();
                
                int totalTickets = totalTicketsResult.Success ? (int)totalTicketsResult.Data : 0;
                int totalParticipants = totalParticipantsResult.Success ? (int)totalParticipantsResult.Data : 0;
                
                double odds = totalTickets > 0 ? (1.0 / totalTickets) * 100 : 0;

                var response = new CurrentLotteryResponse
                {
                    Id = lottery.Id,
                    StartDate = lottery.StartDate,
                    EndDate = lottery.EndDate,
                    Pool = lottery.Pool,
                    TotalTickets = totalTickets,
                    TotalParticipants = totalParticipants,
                    Odds = odds,
                    WinningTicket = lottery.WinningTicket
                };

                return Ok(new ApiResponse<CurrentLotteryResponse>
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
