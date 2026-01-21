using Fitz.Api.Controllers.Account.Exceptions;

namespace Fitz.Api.Controllers.Account.SetSafeBalance.Domain;

public class SetSafeBalanceService(ISetSafeBalance setSafeBalance, ILogger<SetSafeBalanceService> logger)
{
    private readonly ISetSafeBalance _setSafeBalance = setSafeBalance;
    private readonly ILogger<SetSafeBalanceService> _logger = logger;

    public async Task<SetSafeBalanceModel> ExecuteAsync(SetSafeBalanceCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("SetSafeBalanceService execution started. UserId: {UserId}, SafeBalance: {SafeBalance}", command.UserId, command.SafeBalance);

        if (command.UserId == 0)
        {
            _logger.LogError("SetSafeBalance validation failed - User ID cannot be 0.");
            throw new ArgumentException("User ID cannot be 0.", nameof(command.UserId));
        }

        if (command.SafeBalance < 0)
        {
            _logger.LogError("SetSafeBalance validation failed - Safe balance cannot be negative. SafeBalance: {SafeBalance}", command.SafeBalance);
            throw new ArgumentException("Safe balance cannot be negative.", nameof(command.SafeBalance));
        }

        var account = await _setSafeBalance.FindAccountByIdAsync(command.UserId, cancellationToken);
        if (account == null)
        {
            _logger.LogWarning("Account not found. UserId: {UserId}", command.UserId);
            throw new AccountNotFound(command.UserId);
        }

        account.safeBalance = command.SafeBalance;
        await _setSafeBalance.UpdateAccountAsync(account, cancellationToken);

        var model = SetSafeBalanceModel.From(account, command.SafeBalance);

        _logger.LogInformation("SetSafeBalanceModel created successfully. UserId: {UserId}, SafeBalance: {SafeBalance}", command.UserId, command.SafeBalance);

        return model;
    }
}
