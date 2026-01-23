namespace Fitz.Api.Controllers.Lottery.GetLotteryStatistics.Domain;

public record GetLotteryStatisticsResponse(
    List<LotteryStatisticsPointModel> DataPoints,
    double? AverageTicketsPerWinner)
{
    public static GetLotteryStatisticsResponse From(GetLotteryStatisticsModel model)
    {
        return new GetLotteryStatisticsResponse(
            DataPoints: model.DataPoints,
            AverageTicketsPerWinner: model.AverageTicketsPerWinner
        );
    }
}
