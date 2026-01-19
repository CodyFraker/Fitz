using Fitz.Api.Controllers.Lottery.Exceptions;

namespace Fitz.Api.Controllers.Lottery.GetCurrentLottery.Domain;

public class GetCurrentLotteryService(IGetCurrentLottery getCurrentLottery, ILogger<GetCurrentLotteryService> logger)
{
    private readonly IGetCurrentLottery _getCurrentLottery = getCurrentLottery;
    private readonly ILogger<GetCurrentLotteryService> _logger = logger;

    public async Task<GetCurrentLotteryModel> ExecuteAsync(GetCurrentLotteryCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetCurrentLotteryService execution started");

        var lottery = await _getCurrentLottery.FindCurrentAsync(cancellationToken);
        if (lottery == null)
        {
            _logger.LogWarning("Current lottery not found");
            throw new LotteryNotFound();
        }

        _logger.LogInformation("Current lottery found. LotteryId: {LotteryId}", lottery.Id);

        var totalTickets = await _getCurrentLottery.GetTotalTicketsAsync(lottery.Id, cancellationToken);
        var totalParticipants = await _getCurrentLottery.GetTotalParticipantsAsync(lottery.Id, cancellationToken);

        var model = GetCurrentLotteryModel.From(lottery, totalTickets, totalParticipants);

        _logger.LogInformation("GetCurrentLotteryModel created successfully. LotteryId: {LotteryId}, TotalTickets: {TotalTickets}, TotalParticipants: {TotalParticipants}", lottery.Id, totalTickets, totalParticipants);

        return model;
    }
}
