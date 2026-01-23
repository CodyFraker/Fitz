namespace Fitz.Api.Controllers.Lottery.GetLotteryStatistics.Domain;

public record LotteryStatisticsPointModel(
    DateTime Date,
    int PrizePool,
    int TotalTickets);

public record GetLotteryStatisticsModel(
    List<LotteryStatisticsPointModel> DataPoints,
    double? AverageTicketsPerWinner)
{
    public static GetLotteryStatisticsModel From(List<LotteryStatisticsPointModel> dataPoints, double? averageTicketsPerWinner)
    {
        return new GetLotteryStatisticsModel(
            DataPoints: dataPoints,
            AverageTicketsPerWinner: averageTicketsPerWinner
        );
    }
}
