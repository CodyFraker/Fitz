namespace Fitz.Api.Controllers.Lottery.GetLotteryStatistics.Domain;

public record LotteryStatisticsPointModel(
    DateTime Date,
    int PrizePool,
    int TotalTickets);

public record GetLotteryStatisticsModel(
    List<LotteryStatisticsPointModel> DataPoints)
{
    public static GetLotteryStatisticsModel From(List<LotteryStatisticsPointModel> dataPoints)
    {
        return new GetLotteryStatisticsModel(
            DataPoints: dataPoints
        );
    }
}
