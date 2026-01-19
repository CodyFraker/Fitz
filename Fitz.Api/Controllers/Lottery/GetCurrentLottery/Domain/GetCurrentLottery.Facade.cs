namespace Fitz.Api.Controllers.Lottery.GetCurrentLottery.Domain;

public class GetCurrentLotteryFacade(GetCurrentLotteryService getCurrentLotteryService, ILogger<GetCurrentLotteryFacade> logger)
{
    private readonly GetCurrentLotteryService _getCurrentLotteryService = getCurrentLotteryService;
    private readonly ILogger<GetCurrentLotteryFacade> _logger = logger;

    public async Task<GetCurrentLotteryResponse> Execute(GetCurrentLotteryCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetCurrentLotteryFacade execution started");

        var model = await _getCurrentLotteryService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("GetCurrentLotteryService execution completed. LotteryId: {LotteryId}", model.Id);

        var response = GetCurrentLotteryResponse.From(model);

        _logger.LogInformation("GetCurrentLotteryFacade execution completed successfully. LotteryId: {LotteryId}", model.Id);

        return response;
    }
}
