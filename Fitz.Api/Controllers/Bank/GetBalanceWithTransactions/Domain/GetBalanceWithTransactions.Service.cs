using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Bank.GetBalance.Domain;
using Fitz.Api.Controllers.Bank.GetUserTransactions.Domain;

namespace Fitz.Api.Controllers.Bank.GetBalanceWithTransactions.Domain;

public class GetBalanceWithTransactionsService(
    GetBalanceService getBalanceService,
    GetUserTransactionsFacade getUserTransactionsFacade,
    ILogger<GetBalanceWithTransactionsService> logger)
{
    private readonly GetBalanceService _getBalanceService = getBalanceService;
    private readonly GetUserTransactionsFacade _getUserTransactionsFacade = getUserTransactionsFacade;
    private readonly ILogger<GetBalanceWithTransactionsService> _logger = logger;

    public async Task<GetBalanceWithTransactionsModel> ExecuteAsync(GetBalanceWithTransactionsCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetBalanceWithTransactionsService execution started. UserId: {UserId}", command.UserId);

        if (command.UserId == 0)
        {
            _logger.LogError("GetBalanceWithTransactions validation failed - User ID cannot be 0.");
            throw new ArgumentException("User ID cannot be 0.", nameof(command.UserId));
        }

        var balanceCommand = GetBalanceCommand.From(command.UserId);
        var balanceModel = await _getBalanceService.ExecuteAsync(balanceCommand, cancellationToken);

        var transactionsCommand = GetUserTransactionsCommand.From(command.UserId, 0, 10);
        var transactionsResponse = await _getUserTransactionsFacade.Execute(transactionsCommand, cancellationToken);

        var transactionsModel = GetUserTransactionsModel.From(transactionsResponse.Transactions, transactionsResponse.TotalCount);

        var model = GetBalanceWithTransactionsModel.From(balanceModel, transactionsModel);

        _logger.LogInformation("GetBalanceWithTransactionsModel created successfully. UserId: {UserId}, Beer: {Beer}, TransactionCount: {TransactionCount}", 
            command.UserId, model.Account.Beer, model.Transactions.Count);

        return model;
    }
}
