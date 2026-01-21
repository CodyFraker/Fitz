namespace Fitz.Api.Controllers.Bank.GetUserTransactions.Domain;

public class GetUserTransactionsFacade(GetUserTransactionsService getUserTransactionsService, ILogger<GetUserTransactionsFacade> logger)
{
    private readonly GetUserTransactionsService _getUserTransactionsService = getUserTransactionsService;
    private readonly ILogger<GetUserTransactionsFacade> _logger = logger;

    public async Task<GetUserTransactionsResponse> Execute(GetUserTransactionsCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetUserTransactionsFacade execution started. UserId: {UserId}, Skip: {Skip}, Take: {Take}", command.UserId, command.Skip, command.Take);

        var model = await _getUserTransactionsService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("GetUserTransactionsService execution completed. Count: {Count}, TotalCount: {TotalCount}", model.Transactions.Count, model.TotalCount);

        var response = GetUserTransactionsResponse.From(model);

        _logger.LogInformation("GetUserTransactionsFacade execution completed successfully. Count: {Count}, TotalCount: {TotalCount}", model.Transactions.Count, model.TotalCount);

        return response;
    }
}
