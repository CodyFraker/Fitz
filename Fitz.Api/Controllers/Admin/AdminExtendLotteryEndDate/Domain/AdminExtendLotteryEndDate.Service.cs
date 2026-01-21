using Fitz.Api.Controllers.Lottery.Exceptions;
using Fitz.Features.Lottery;

namespace Fitz.Api.Controllers.Admin.AdminExtendLotteryEndDate.Domain;

public class AdminExtendLotteryEndDateService(
    IAdminExtendLotteryEndDate adminExtendLotteryEndDate,
    LotteryService lotteryService,
    ILogger<AdminExtendLotteryEndDateService> logger)
{
    private readonly IAdminExtendLotteryEndDate _adminExtendLotteryEndDate = adminExtendLotteryEndDate;
    private readonly LotteryService _lotteryService = lotteryService;
    private readonly ILogger<AdminExtendLotteryEndDateService> _logger = logger;

    public async Task<AdminExtendLotteryEndDateModel> ExecuteAsync(AdminExtendLotteryEndDateCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AdminExtendLotteryEndDateService execution started. EndDate: {EndDate}", command.EndDate);

        var currentLottery = await _adminExtendLotteryEndDate.GetCurrentLotteryAsync(cancellationToken);
        if (currentLottery == null)
        {
            _logger.LogWarning("Current lottery not found");
            throw new LotteryNotFound();
        }

        if (command.EndDate <= currentLottery.StartDate)
        {
            _logger.LogError("AdminExtendLotteryEndDate validation failed - EndDate must be after StartDate. StartDate: {StartDate}, EndDate: {EndDate}", 
                currentLottery.StartDate, command.EndDate);
            throw new ArgumentException("EndDate must be after StartDate.", nameof(command.EndDate));
        }

        await _lotteryService.UpdateCurrentLottery(command.EndDate);

        var model = AdminExtendLotteryEndDateModel.From("Lottery end date extended successfully");

        _logger.LogInformation("AdminExtendLotteryEndDateModel created successfully");

        return model;
    }
}
