namespace Fitz.Api.Controllers.Bank.GetUserTransactions.Domain;

public class GetUserTransactionsService(IGetUserTransactions getUserTransactions, ILogger<GetUserTransactionsService> logger)
{
    private readonly IGetUserTransactions _getUserTransactions = getUserTransactions;
    private readonly ILogger<GetUserTransactionsService> _logger = logger;

    public async Task<GetUserTransactionsModel> ExecuteAsync(GetUserTransactionsCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetUserTransactionsService execution started. UserId: {UserId}, Skip: {Skip}, Take: {Take}", command.UserId, command.Skip, command.Take);

        if (command.UserId == 0)
        {
            _logger.LogError("GetUserTransactions validation failed - User ID cannot be 0.");
            throw new ArgumentException("User ID cannot be 0.", nameof(command.UserId));
        }

        if (command.Skip < 0)
        {
            _logger.LogError("GetUserTransactions validation failed - Skip cannot be negative. Skip: {Skip}", command.Skip);
            throw new ArgumentException("Skip cannot be negative.", nameof(command.Skip));
        }

        if (command.Take < 1 || command.Take > 100)
        {
            _logger.LogError("GetUserTransactions validation failed - Take must be between 1 and 100. Take: {Take}", command.Take);
            throw new ArgumentException("Take must be between 1 and 100.", nameof(command.Take));
        }

        var (transactions, totalCount) = await _getUserTransactions.GetUserTransactionsAsync(command.UserId, command.Skip, command.Take, cancellationToken);

        var model = GetUserTransactionsModel.From(transactions, totalCount);

        _logger.LogInformation("GetUserTransactionsModel created successfully. Count: {Count}, TotalCount: {TotalCount}", transactions.Count, totalCount);

        return model;
    }
}
