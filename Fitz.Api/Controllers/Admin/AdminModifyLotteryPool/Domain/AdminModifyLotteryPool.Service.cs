using Fitz.Api.Controllers.Lottery.Exceptions;
using Fitz.Features.Lottery;

namespace Fitz.Api.Controllers.Admin.AdminModifyLotteryPool.Domain;

public class AdminModifyLotteryPoolService(
    IAdminModifyLotteryPool adminModifyLotteryPool,
    LotteryService lotteryService,
    ILogger<AdminModifyLotteryPoolService> logger)
{
    private readonly IAdminModifyLotteryPool _adminModifyLotteryPool = adminModifyLotteryPool;
    private readonly LotteryService _lotteryService = lotteryService;
    private readonly ILogger<AdminModifyLotteryPoolService> _logger = logger;

    public async Task<AdminModifyLotteryPoolModel> ExecuteAsync(AdminModifyLotteryPoolCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AdminModifyLotteryPoolService execution started. Pool: {Pool}", command.Pool);

        if (command.Pool < 0)
        {
            _logger.LogError("AdminModifyLotteryPool validation failed - Pool must be greater than or equal to 0. Pool: {Pool}", command.Pool);
            throw new ArgumentException("Pool must be greater than or equal to 0.", nameof(command.Pool));
        }

        var currentLottery = await _adminModifyLotteryPool.GetCurrentLotteryAsync(cancellationToken);
        if (currentLottery == null)
        {
            _logger.LogWarning("Current lottery not found");
            throw new LotteryNotFound();
        }

        var result = await _lotteryService.SetLotteryPrizePoolAsync(command.Pool);
        if (!result.Success)
        {
            _logger.LogError("Failed to set lottery prize pool. Message: {Message}", result.Message);
            throw new InvalidOperationException(result.Message);
        }

        var model = AdminModifyLotteryPoolModel.From(result.Message);

        _logger.LogInformation("AdminModifyLotteryPoolModel created successfully. Message: {Message}", result.Message);

        return model;
    }
}
