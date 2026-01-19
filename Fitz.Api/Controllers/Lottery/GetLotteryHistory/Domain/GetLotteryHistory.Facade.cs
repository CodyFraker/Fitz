namespace Fitz.Api.Controllers.Lottery.GetLotteryHistory.Domain;

public class GetLotteryHistoryFacade(GetLotteryHistoryService getLotteryHistoryService, ILogger<GetLotteryHistoryFacade> logger)
{
    private readonly GetLotteryHistoryService _getLotteryHistoryService = getLotteryHistoryService;
    private readonly ILogger<GetLotteryHistoryFacade> _logger = logger;

    public async Task<GetLotteryHistoryResponse> Execute(GetLotteryHistoryCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetLotteryHistoryFacade execution started. Skip: {Skip}, Take: {Take}", command.Skip, command.Take);

        var model = await _getLotteryHistoryService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("GetLotteryHistoryService execution completed. TotalCount: {TotalCount}, ItemsReturned: {ItemsReturned}", model.TotalCount, model.Lotteries.Count);

        var response = GetLotteryHistoryResponse.From(model);

        _logger.LogInformation("GetLotteryHistoryFacade execution completed successfully. TotalCount: {TotalCount}, ItemsReturned: {ItemsReturned}", model.TotalCount, model.Lotteries.Count);

        return response;
    }
}
