namespace Fitz.Api.Controllers.Bank.GetTopBalances.Domain;

public class GetTopBalancesFacade(GetTopBalancesService getTopBalancesService, ILogger<GetTopBalancesFacade> logger)
{
    private readonly GetTopBalancesService _getTopBalancesService = getTopBalancesService;
    private readonly ILogger<GetTopBalancesFacade> _logger = logger;

    public async Task<GetTopBalancesResponse> Execute(GetTopBalancesCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetTopBalancesFacade execution started. Limit: {Limit}", command.Limit);

        var model = await _getTopBalancesService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("GetTopBalancesService execution completed. Count: {Count}", model.Accounts.Count);

        var response = GetTopBalancesResponse.From(model);

        _logger.LogInformation("GetTopBalancesFacade execution completed successfully. Count: {Count}", model.Accounts.Count);

        return response;
    }
}
