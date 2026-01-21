namespace Fitz.Api.Controllers.Bank.GetBalances.Domain;

public class GetBalancesService(IGetBalances getBalances, ILogger<GetBalancesService> logger)
{
    private readonly IGetBalances _getBalances = getBalances;
    private readonly ILogger<GetBalancesService> _logger = logger;

    public async Task<GetBalancesModel> ExecuteAsync(GetBalancesCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetBalancesService execution started. Skip: {Skip}, Take: {Take}", command.Skip, command.Take);

        if (command.Skip < 0)
        {
            _logger.LogError("GetBalances validation failed - Skip cannot be negative. Skip: {Skip}", command.Skip);
            throw new ArgumentException("Skip cannot be negative.", nameof(command.Skip));
        }

        if (command.Take <= 0)
        {
            _logger.LogError("GetBalances validation failed - Take must be greater than 0. Take: {Take}", command.Take);
            throw new ArgumentException("Take must be greater than 0.", nameof(command.Take));
        }

        var (accounts, totalCount) = await _getBalances.GetBalancesAsync(command.Skip, command.Take, cancellationToken);

        var model = GetBalancesModel.From(accounts, totalCount);

        _logger.LogInformation("GetBalancesModel created successfully. Count: {Count}, TotalCount: {TotalCount}", accounts.Count, totalCount);

        return model;
    }
}
