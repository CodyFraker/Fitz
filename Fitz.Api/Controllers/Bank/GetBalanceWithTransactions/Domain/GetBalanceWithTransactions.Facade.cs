namespace Fitz.Api.Controllers.Bank.GetBalanceWithTransactions.Domain;

public class GetBalanceWithTransactionsFacade(GetBalanceWithTransactionsService getBalanceWithTransactionsService, ILogger<GetBalanceWithTransactionsFacade> logger)
{
    private readonly GetBalanceWithTransactionsService _getBalanceWithTransactionsService = getBalanceWithTransactionsService;
    private readonly ILogger<GetBalanceWithTransactionsFacade> _logger = logger;

    public async Task<GetBalanceWithTransactionsModel> Execute(GetBalanceWithTransactionsCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetBalanceWithTransactionsFacade execution started. UserId: {UserId}", command.UserId);

        var model = await _getBalanceWithTransactionsService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("GetBalanceWithTransactionsFacade execution completed successfully. UserId: {UserId}, Beer: {Beer}, TransactionCount: {TransactionCount}", 
            command.UserId, model.Account.Beer, model.Transactions.Count);

        return model;
    }
}
