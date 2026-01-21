using Fitz.Features.Lottery;

namespace Fitz.Api.Controllers.Admin.AdminCreateLottery.Domain;

public class AdminCreateLotteryService(
    LotteryService lotteryService,
    ILogger<AdminCreateLotteryService> logger)
{
    private readonly LotteryService _lotteryService = lotteryService;
    private readonly ILogger<AdminCreateLotteryService> _logger = logger;

    public async Task<AdminCreateLotteryModel> ExecuteAsync(AdminCreateLotteryCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AdminCreateLotteryService execution started. StartDate: {StartDate}, EndDate: {EndDate}, Pool: {Pool}", 
            command.StartDate, command.EndDate, command.Pool);

        var startDate = command.StartDate ?? DateTime.UtcNow;
        var endDate = command.EndDate ?? DateTime.UtcNow.AddDays(7);

        if (endDate <= startDate)
        {
            _logger.LogError("AdminCreateLottery validation failed - EndDate must be after StartDate. StartDate: {StartDate}, EndDate: {EndDate}", 
                startDate, endDate);
            throw new ArgumentException("EndDate must be after StartDate.", nameof(command.EndDate));
        }

        if (command.Pool < 0)
        {
            _logger.LogError("AdminCreateLottery validation failed - Pool must be greater than or equal to 0. Pool: {Pool}", command.Pool);
            throw new ArgumentException("Pool must be greater than or equal to 0.", nameof(command.Pool));
        }

        await _lotteryService.StartNewLotteryAsync(startDate, endDate, command.Pool);

        var model = AdminCreateLotteryModel.From("Lottery created successfully");

        _logger.LogInformation("AdminCreateLotteryModel created successfully");

        return model;
    }
}
