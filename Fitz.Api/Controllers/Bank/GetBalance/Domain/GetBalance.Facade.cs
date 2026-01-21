namespace Fitz.Api.Controllers.Bank.GetBalance.Domain;

public class GetBalanceFacade(GetBalanceService getBalanceService, ILogger<GetBalanceFacade> logger)
{
    private readonly GetBalanceService _getBalanceService = getBalanceService;
    private readonly ILogger<GetBalanceFacade> _logger = logger;

    public async Task<GetBalanceResponse> Execute(GetBalanceCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetBalanceFacade execution started. UserId: {UserId}", command.UserId);

        var model = await _getBalanceService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("GetBalanceService execution completed. UserId: {UserId}, Beer: {Beer}", command.UserId, model.Account.Beer);

        var response = GetBalanceResponse.From(model);

        _logger.LogInformation("GetBalanceFacade execution completed successfully. UserId: {UserId}, Beer: {Beer}", command.UserId, model.Account.Beer);

        return response;
    }
}
