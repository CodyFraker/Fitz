namespace Fitz.Api.Controllers.Bank.GetTopBalances.Domain;

public class GetTopBalancesService(IGetTopBalances getTopBalances, ILogger<GetTopBalancesService> logger)
{
    private readonly IGetTopBalances _getTopBalances = getTopBalances;
    private readonly ILogger<GetTopBalancesService> _logger = logger;

    public async Task<GetTopBalancesModel> ExecuteAsync(GetTopBalancesCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetTopBalancesService execution started. Limit: {Limit}", command.Limit);

        if (command.Limit <= 0)
        {
            _logger.LogError("GetTopBalances validation failed - Limit must be greater than 0. Limit: {Limit}", command.Limit);
            throw new ArgumentException("Limit must be greater than 0.", nameof(command.Limit));
        }

        var accounts = await _getTopBalances.GetTopBalancesAsync(command.Limit, cancellationToken);

        var model = GetTopBalancesModel.From(accounts);

        _logger.LogInformation("GetTopBalancesModel created successfully. Count: {Count}", accounts.Count);

        return model;
    }
}
