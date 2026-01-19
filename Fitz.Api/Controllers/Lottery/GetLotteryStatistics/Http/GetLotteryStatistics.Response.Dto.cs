using Fitz.Api.Controllers.Lottery.GetLotteryStatistics.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Lottery.GetLotteryStatistics.Http;

[DisplayName("LotteryStatisticsPoint")]
public record LotteryStatisticsPointDto
{
    [Required]
    public required DateTime Date { get; set; }

    [Required]
    public required int PrizePool { get; set; }

    [Required]
    public required int TotalTickets { get; set; }
}

[DisplayName("GetLotteryStatisticsResponse")]
public record GetLotteryStatisticsResponseDto
{
    [Required]
    public required List<LotteryStatisticsPointDto> DataPoints { get; set; }

    public static GetLotteryStatisticsResponseDto From(GetLotteryStatisticsResponse response)
    {
        return new GetLotteryStatisticsResponseDto
        {
            DataPoints = response.DataPoints.Select(p => new LotteryStatisticsPointDto
            {
                Date = p.Date,
                PrizePool = p.PrizePool,
                TotalTickets = p.TotalTickets
            }).ToList()
        };
    }
}
