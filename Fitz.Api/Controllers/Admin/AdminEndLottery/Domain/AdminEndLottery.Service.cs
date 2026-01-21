using Fitz.Api.Controllers.Lottery.Exceptions;
using Fitz.Features.Lottery;

namespace Fitz.Api.Controllers.Admin.AdminEndLottery.Domain;

public class AdminEndLotteryService(
    IAdminEndLottery adminEndLottery,
    LotteryService lotteryService,
    ILogger<AdminEndLotteryService> logger)
{
    private readonly IAdminEndLottery _adminEndLottery = adminEndLottery;
    private readonly LotteryService _lotteryService = lotteryService;
    private readonly ILogger<AdminEndLotteryService> _logger = logger;

    public async Task<AdminEndLotteryModel> ExecuteAsync(AdminEndLotteryCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AdminEndLotteryService execution started");

        var currentLottery = await _adminEndLottery.GetCurrentLotteryAsync(cancellationToken);
        if (currentLottery == null)
        {
            _logger.LogWarning("Current lottery not found");
            throw new LotteryNotFound();
        }

        await _lotteryService.EndLotteryAndDecideWinnersAsync(currentLottery);

        var model = AdminEndLotteryModel.From("Lottery ended successfully and winners have been determined");

        _logger.LogInformation("AdminEndLotteryModel created successfully");

        return model;
    }
}
