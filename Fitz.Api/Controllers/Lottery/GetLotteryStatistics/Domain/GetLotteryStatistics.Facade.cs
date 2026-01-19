namespace Fitz.Api.Controllers.Lottery.GetLotteryStatistics.Domain;

public class GetLotteryStatisticsFacade(GetLotteryStatisticsService getLotteryStatisticsService, ILogger<GetLotteryStatisticsFacade> logger)
{
    private readonly GetLotteryStatisticsService _getLotteryStatisticsService = getLotteryStatisticsService;
    private readonly ILogger<GetLotteryStatisticsFacade> _logger = logger;

    public async Task<GetLotteryStatisticsResponse> Execute(GetLotteryStatisticsCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetLotteryStatisticsFacade execution started");

        var model = await _getLotteryStatisticsService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("GetLotteryStatisticsService execution completed. DataPointsCount: {DataPointsCount}", model.DataPoints.Count);

        var response = GetLotteryStatisticsResponse.From(model);

        _logger.LogInformation("GetLotteryStatisticsFacade execution completed successfully. DataPointsCount: {DataPointsCount}", model.DataPoints.Count);

        return response;
    }
}
