namespace Fitz.Api.Controllers.Lottery.GetLotteryStatistics.Domain;

public record GetLotteryStatisticsCommand
{
    public static GetLotteryStatisticsCommand From()
    {
        return new GetLotteryStatisticsCommand();
    }
}
