using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Database.Entities;
using Fitz.Metrics;

namespace Fitz.Api.Controllers.Bank.AwardBonus.Domain;

public class AwardBonusService(IAwardBonus awardBonus, FitzMetrics? fitzMetrics, ILogger<AwardBonusService> logger)
{
    private readonly IAwardBonus _awardBonus = awardBonus;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;
    private readonly ILogger<AwardBonusService> _logger = logger;

    public async Task<AwardBonusModel> ExecuteAsync(AwardBonusCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AwardBonusService execution started. UserId: {UserId}, Amount: {Amount}", command.UserId, command.Amount);

        if (command.UserId == 0)
        {
            _logger.LogError("AwardBonus validation failed - User ID cannot be 0.");
            throw new ArgumentException("User ID cannot be 0.", nameof(command.UserId));
        }

        if (command.Amount <= 0)
        {
            _logger.LogError("AwardBonus validation failed - Amount must be greater than 0. Amount: {Amount}", command.Amount);
            throw new ArgumentException("Amount must be greater than 0.", nameof(command.Amount));
        }

        var account = await _awardBonus.FindAccountByIdAsync(command.UserId, cancellationToken);
        if (account == null)
        {
            _logger.LogWarning("Account not found. UserId: {UserId}", command.UserId);
            throw new AccountNotFound(command.UserId);
        }

        account.Beer += command.Amount;
        account.LifetimeBeer += command.Amount;
        await _awardBonus.UpdateAccountAsync(account, cancellationToken);

        await _awardBonus.LogTransactionAsync(account.Id, account.Id, command.Amount, Reason.Bonus, cancellationToken);

        _fitzMetrics?.RecordBeerAward(command.Amount, Reason.Bonus.ToString());
        _fitzMetrics?.RecordTransaction("award");

        var model = AwardBonusModel.From(account, command.Amount);

        _logger.LogInformation("AwardBonusModel created successfully. UserId: {UserId}, Amount: {Amount}, NewBalance: {NewBalance}", command.UserId, command.Amount, account.Beer);

        return model;
    }
}
