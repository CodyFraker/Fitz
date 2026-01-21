namespace Fitz.Api.Controllers.Admin.AdminModifyLotteryPool.Domain;

public class AdminModifyLotteryPoolFacade(AdminModifyLotteryPoolService adminModifyLotteryPoolService, ILogger<AdminModifyLotteryPoolFacade> logger)
{
    private readonly AdminModifyLotteryPoolService _adminModifyLotteryPoolService = adminModifyLotteryPoolService;
    private readonly ILogger<AdminModifyLotteryPoolFacade> _logger = logger;

    public async Task<AdminModifyLotteryPoolResponse> Execute(AdminModifyLotteryPoolCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AdminModifyLotteryPoolFacade execution started. Pool: {Pool}", command.Pool);

        var model = await _adminModifyLotteryPoolService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("AdminModifyLotteryPoolService execution completed. Message: {Message}", model.Message);

        var response = AdminModifyLotteryPoolResponse.From(model);

        _logger.LogInformation("AdminModifyLotteryPoolFacade execution completed successfully. Message: {Message}", model.Message);

        return response;
    }
}
