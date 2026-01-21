namespace Fitz.Api.Controllers.Account.SetSafeBalance.Domain;

public class SetSafeBalanceFacade(SetSafeBalanceService setSafeBalanceService, ILogger<SetSafeBalanceFacade> logger)
{
    private readonly SetSafeBalanceService _setSafeBalanceService = setSafeBalanceService;
    private readonly ILogger<SetSafeBalanceFacade> _logger = logger;

    public async Task<SetSafeBalanceResponse> Execute(SetSafeBalanceCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("SetSafeBalanceFacade execution started. UserId: {UserId}, SafeBalance: {SafeBalance}", command.UserId, command.SafeBalance);

        var model = await _setSafeBalanceService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("SetSafeBalanceService execution completed. UserId: {UserId}, SafeBalance: {SafeBalance}", command.UserId, command.SafeBalance);

        var response = SetSafeBalanceResponse.From(model);

        _logger.LogInformation("SetSafeBalanceFacade execution completed successfully. UserId: {UserId}, SafeBalance: {SafeBalance}", command.UserId, command.SafeBalance);

        return response;
    }
}
