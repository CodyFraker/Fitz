namespace Fitz.Api.Controllers.Admin.AdminExtendLotteryEndDate.Domain;

public class AdminExtendLotteryEndDateFacade(AdminExtendLotteryEndDateService adminExtendLotteryEndDateService, ILogger<AdminExtendLotteryEndDateFacade> logger)
{
    private readonly AdminExtendLotteryEndDateService _adminExtendLotteryEndDateService = adminExtendLotteryEndDateService;
    private readonly ILogger<AdminExtendLotteryEndDateFacade> _logger = logger;

    public async Task<AdminExtendLotteryEndDateResponse> Execute(AdminExtendLotteryEndDateCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AdminExtendLotteryEndDateFacade execution started. EndDate: {EndDate}", command.EndDate);

        var model = await _adminExtendLotteryEndDateService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("AdminExtendLotteryEndDateService execution completed. Message: {Message}", model.Message);

        var response = AdminExtendLotteryEndDateResponse.From(model);

        _logger.LogInformation("AdminExtendLotteryEndDateFacade execution completed successfully. Message: {Message}", model.Message);

        return response;
    }
}
