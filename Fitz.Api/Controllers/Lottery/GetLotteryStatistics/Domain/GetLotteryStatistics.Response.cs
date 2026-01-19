namespace Fitz.Api.Controllers.Lottery.GetLotteryStatistics.Domain;

public record GetLotteryStatisticsResponse(
    List<LotteryStatisticsPointModel> DataPoints)
{
    public static GetLotteryStatisticsResponse From(GetLotteryStatisticsModel model)
    {
        return new GetLotteryStatisticsResponse(
            DataPoints: model.DataPoints
        );
    }
}
