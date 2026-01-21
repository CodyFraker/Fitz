namespace Fitz.Api.Controllers.Bank.GetTransactions.Domain;

public class GetTransactionsService(IGetTransactions getTransactions, ILogger<GetTransactionsService> logger)
{
    private readonly IGetTransactions _getTransactions = getTransactions;
    private readonly ILogger<GetTransactionsService> _logger = logger;

    public async Task<GetTransactionsModel> ExecuteAsync(GetTransactionsCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetTransactionsService execution started. Take: {Take}", command.Take);

        if (command.Take <= 0)
        {
            _logger.LogError("GetTransactions validation failed - Take must be greater than 0. Take: {Take}", command.Take);
            throw new ArgumentException("Take must be greater than 0.", nameof(command.Take));
        }

        var transactions = await _getTransactions.GetTransactionsAsync(command.Take, cancellationToken);

        var model = GetTransactionsModel.From(transactions);

        _logger.LogInformation("GetTransactionsModel created successfully. Count: {Count}", transactions.Count);

        return model;
    }
}
