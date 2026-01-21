namespace Fitz.Api.Controllers.Bank.GetBalances.Domain;

public class GetBalancesFacade(GetBalancesService getBalancesService, ILogger<GetBalancesFacade> logger)
{
    private readonly GetBalancesService _getBalancesService = getBalancesService;
    private readonly ILogger<GetBalancesFacade> _logger = logger;

    public async Task<GetBalancesResponse> Execute(GetBalancesCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetBalancesFacade execution started. Skip: {Skip}, Take: {Take}", command.Skip, command.Take);

        var model = await _getBalancesService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("GetBalancesService execution completed. Count: {Count}, TotalCount: {TotalCount}", model.Accounts.Count, model.TotalCount);

        var response = GetBalancesResponse.From(model);

        _logger.LogInformation("GetBalancesFacade execution completed successfully. Count: {Count}, TotalCount: {TotalCount}", model.Accounts.Count, model.TotalCount);

        return response;
    }
}
