namespace Fitz.Api.Controllers.Admin.AdminEndLottery.Domain;

public class AdminEndLotteryFacade(AdminEndLotteryService adminEndLotteryService, ILogger<AdminEndLotteryFacade> logger)
{
    private readonly AdminEndLotteryService _adminEndLotteryService = adminEndLotteryService;
    private readonly ILogger<AdminEndLotteryFacade> _logger = logger;

    public async Task<AdminEndLotteryResponse> Execute(AdminEndLotteryCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AdminEndLotteryFacade execution started");

        var model = await _adminEndLotteryService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("AdminEndLotteryService execution completed. Message: {Message}", model.Message);

        var response = AdminEndLotteryResponse.From(model);

        _logger.LogInformation("AdminEndLotteryFacade execution completed successfully. Message: {Message}", model.Message);

        return response;
    }
}
