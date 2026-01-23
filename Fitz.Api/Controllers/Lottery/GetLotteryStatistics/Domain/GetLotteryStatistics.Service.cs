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

        double? averageTicketsPerWinner = null;
        var allWinners = await _getLotteryStatistics.GetAllWinnersAsync(cancellationToken);

        if (allWinners.Count > 0)
        {
            int totalWinnerTickets = 0;
            foreach (var winner in allWinners)
            {
                var ticketCount = await _getLotteryStatistics.GetTicketCountForWinnerAsync(winner.Drawing, winner.AccountId, cancellationToken);
                totalWinnerTickets += ticketCount;
            }

            averageTicketsPerWinner = (double)totalWinnerTickets / allWinners.Count;
            _logger.LogInformation("Average tickets per winner calculated. TotalWinners: {TotalWinners}, TotalTickets: {TotalTickets}, Average: {Average}", allWinners.Count, totalWinnerTickets, averageTicketsPerWinner);
        }
        else
        {
            _logger.LogInformation("No winners found, average tickets per winner will be null");
        }

        var model = GetLotteryStatisticsModel.From(dataPoints, averageTicketsPerWinner);

        _logger.LogInformation("GetLotteryStatisticsModel created successfully. DataPointsCount: {DataPointsCount}", dataPoints.Count);

        return model;
    }
}
