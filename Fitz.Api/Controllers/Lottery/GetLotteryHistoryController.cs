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
    public class GetLotteryHistoryController : ControllerBase
    {
        private readonly LotteryService _lotteryService;
        private readonly FitzMetrics? _fitzMetrics;

        public GetLotteryHistoryController(LotteryService lotteryService, FitzMetrics? fitzMetrics = null)
        {
            _lotteryService = lotteryService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpGet("history")]
        [RequireDiscordAuth]
        public IActionResult GetLotteryHistory([FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/lottery/history";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                var (lotteries, totalCount) = _lotteryService.GetLotteryHistory(skip, take);

                var historyItems = lotteries.Select(lottery =>
                {
                    var totalTicketsResult = _lotteryService.GetTotalTicketsForLottery(lottery);
                    var totalParticipantsResult = _lotteryService.GetTotalLotteryParticipantsByLottery(lottery);
                    
                    int totalTickets = totalTicketsResult.Success ? (int)totalTicketsResult.Data : 0;
                    int totalParticipants = totalParticipantsResult.Success ? (int)totalParticipantsResult.Data : 0;

                    return new LotteryHistoryItem
                    {
                        Id = lottery.Id,
                        StartDate = lottery.StartDate,
                        EndDate = lottery.EndDate,
                        Pool = lottery.Pool,
                        WinningTicket = lottery.WinningTicket,
                        TotalTickets = totalTickets,
                        TotalParticipants = totalParticipants
                    };
                }).ToList();

                var response = new LotteryHistoryResponse
                {
                    Lotteries = historyItems,
                    TotalCount = totalCount,
                    Skip = skip,
                    Take = take
                };

                return Ok(new ApiResponse<LotteryHistoryResponse>
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
