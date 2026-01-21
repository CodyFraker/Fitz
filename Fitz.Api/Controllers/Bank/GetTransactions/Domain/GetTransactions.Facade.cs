namespace Fitz.Api.Controllers.Bank.GetTransactions.Domain;

public class GetTransactionsFacade(GetTransactionsService getTransactionsService, ILogger<GetTransactionsFacade> logger)
{
    private readonly GetTransactionsService _getTransactionsService = getTransactionsService;
    private readonly ILogger<GetTransactionsFacade> _logger = logger;

    public async Task<GetTransactionsResponse> Execute(GetTransactionsCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetTransactionsFacade execution started. Take: {Take}", command.Take);

        var model = await _getTransactionsService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("GetTransactionsService execution completed. Count: {Count}", model.Transactions.Count);

        var response = GetTransactionsResponse.From(model);

        _logger.LogInformation("GetTransactionsFacade execution completed successfully. Count: {Count}", model.Transactions.Count);

        return response;
    }
}
