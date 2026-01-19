namespace Fitz.Api.Controllers.Lottery.GetLotteryStatistics.Domain;

public class GetLotteryStatisticsService(IGetLotteryStatistics getLotteryStatistics, ILogger<GetLotteryStatisticsService> logger)
{
    private readonly IGetLotteryStatistics _getLotteryStatistics = getLotteryStatistics;
    private readonly ILogger<GetLotteryStatisticsService> _logger = logger;

    public async Task<GetLotteryStatisticsModel> ExecuteAsync(GetLotteryStatisticsCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetLotteryStatisticsService execution started");

        var allLotteries = await _getLotteryStatistics.FindAllLotteriesAsync(cancellationToken);
        var dataPoints = new List<LotteryStatisticsPointModel>();

        foreach (var lottery in allLotteries)
        {
            var totalTickets = await _getLotteryStatistics.GetTotalTicketsAsync(lottery.Id, cancellationToken);
            int prizePool = lottery.Pool ?? 0;

            var point = new LotteryStatisticsPointModel(
                Date: lottery.StartDate,
                PrizePool: prizePool,
                TotalTickets: totalTickets
            );

            dataPoints.Add(point);
        }

        var model = GetLotteryStatisticsModel.From(dataPoints);

        _logger.LogInformation("GetLotteryStatisticsModel created successfully. DataPointsCount: {DataPointsCount}", dataPoints.Count);

        return model;
    }
}
