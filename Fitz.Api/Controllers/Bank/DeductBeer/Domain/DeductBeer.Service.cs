using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Metrics;

namespace Fitz.Api.Controllers.Bank.DeductBeer.Domain;

public class DeductBeerService(IDeductBeer deductBeer, FitzMetrics? fitzMetrics, ILogger<DeductBeerService> logger)
{
    private readonly IDeductBeer _deductBeer = deductBeer;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;
    private readonly ILogger<DeductBeerService> _logger = logger;

    public async Task<DeductBeerModel> ExecuteAsync(DeductBeerCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("DeductBeerService execution started. UserId: {UserId}, Amount: {Amount}, Reason: {Reason}", command.UserId, command.Amount, command.Reason);

        if (command.UserId == 0)
        {
            _logger.LogError("DeductBeer validation failed - User ID cannot be 0.");
            throw new ArgumentException("User ID cannot be 0.", nameof(command.UserId));
        }

        if (command.Amount <= 0)
        {
            _logger.LogError("DeductBeer validation failed - Amount must be greater than 0. Amount: {Amount}", command.Amount);
            throw new ArgumentException("Amount must be greater than 0.", nameof(command.Amount));
        }

        var account = await _deductBeer.FindAccountByIdAsync(command.UserId, cancellationToken);
        if (account == null)
        {
            _logger.LogWarning("Account not found. UserId: {UserId}", command.UserId);
            throw new AccountNotFound(command.UserId);
        }

        if (account.Beer < command.Amount)
        {
            _logger.LogWarning("Insufficient beer. UserId: {UserId}, Required: {Required}, Current: {Current}", command.UserId, command.Amount, account.Beer);
            throw new InvalidOperationException($"User {command.UserId} does not have enough beer to deduct.");
        }

        account.Beer -= command.Amount;
        await _deductBeer.UpdateAccountAsync(account, cancellationToken);

        await _deductBeer.LogTransactionAsync(account.Id, account.Id, command.Amount, command.Reason, cancellationToken);

        _fitzMetrics?.RecordBeerDeduction(command.Amount, command.Reason.ToString());
        _fitzMetrics?.RecordTransaction("deduction");

        var model = DeductBeerModel.From(account, command.Amount);

        _logger.LogInformation("DeductBeerModel created successfully. UserId: {UserId}, Amount: {Amount}, NewBalance: {NewBalance}", command.UserId, command.Amount, account.Beer);

        return model;
    }
}
