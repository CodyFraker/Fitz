using Fitz.Api.Controllers.Lottery.Exceptions;
using Fitz.Features.Lottery;

namespace Fitz.Api.Controllers.Admin.AdminCancelLottery.Domain;

public class AdminCancelLotteryService(
    IAdminCancelLottery adminCancelLottery,
    LotteryService lotteryService,
    ILogger<AdminCancelLotteryService> logger)
{
    private readonly IAdminCancelLottery _adminCancelLottery = adminCancelLottery;
    private readonly LotteryService _lotteryService = lotteryService;
    private readonly ILogger<AdminCancelLotteryService> _logger = logger;

    public async Task<AdminCancelLotteryModel> ExecuteAsync(AdminCancelLotteryCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AdminCancelLotteryService execution started");

        var currentLottery = await _adminCancelLottery.GetCurrentLotteryAsync(cancellationToken);
        if (currentLottery == null)
        {
            _logger.LogWarning("Current lottery not found");
            throw new LotteryNotFound();
        }

        await _lotteryService.EndLotteryAsync(currentLottery);

        var model = AdminCancelLotteryModel.From("Lottery cancelled successfully");

        _logger.LogInformation("AdminCancelLotteryModel created successfully");

        return model;
    }
}
