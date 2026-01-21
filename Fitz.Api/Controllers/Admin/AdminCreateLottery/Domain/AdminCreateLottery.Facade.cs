namespace Fitz.Api.Controllers.Admin.AdminCreateLottery.Domain;

public class AdminCreateLotteryFacade(AdminCreateLotteryService adminCreateLotteryService, ILogger<AdminCreateLotteryFacade> logger)
{
    private readonly AdminCreateLotteryService _adminCreateLotteryService = adminCreateLotteryService;
    private readonly ILogger<AdminCreateLotteryFacade> _logger = logger;

    public async Task<AdminCreateLotteryResponse> Execute(AdminCreateLotteryCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AdminCreateLotteryFacade execution started. StartDate: {StartDate}, EndDate: {EndDate}, Pool: {Pool}", 
            command.StartDate, command.EndDate, command.Pool);

        var model = await _adminCreateLotteryService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("AdminCreateLotteryService execution completed. Message: {Message}", model.Message);

        var response = AdminCreateLotteryResponse.From(model);

        _logger.LogInformation("AdminCreateLotteryFacade execution completed successfully. Message: {Message}", model.Message);

        return response;
    }
}
