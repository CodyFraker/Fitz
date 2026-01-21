namespace Fitz.Api.Controllers.Admin.AdminCancelLottery.Domain;

public class AdminCancelLotteryFacade(AdminCancelLotteryService adminCancelLotteryService, ILogger<AdminCancelLotteryFacade> logger)
{
    private readonly AdminCancelLotteryService _adminCancelLotteryService = adminCancelLotteryService;
    private readonly ILogger<AdminCancelLotteryFacade> _logger = logger;

    public async Task<AdminCancelLotteryResponse> Execute(AdminCancelLotteryCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AdminCancelLotteryFacade execution started");

        var model = await _adminCancelLotteryService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("AdminCancelLotteryService execution completed. Message: {Message}", model.Message);

        var response = AdminCancelLotteryResponse.From(model);

        _logger.LogInformation("AdminCancelLotteryFacade execution completed successfully. Message: {Message}", model.Message);

        return response;
    }
}
