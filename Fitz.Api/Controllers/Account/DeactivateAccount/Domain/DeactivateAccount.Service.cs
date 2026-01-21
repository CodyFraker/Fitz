using Fitz.Api.Controllers.Account.Exceptions;

namespace Fitz.Api.Controllers.Account.DeactivateAccount.Domain;

public class DeactivateAccountService(IDeactivateAccount deactivateAccount, ILogger<DeactivateAccountService> logger)
{
    private readonly IDeactivateAccount _deactivateAccount = deactivateAccount;
    private readonly ILogger<DeactivateAccountService> _logger = logger;

    public async Task<DeactivateAccountModel> ExecuteAsync(DeactivateAccountCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("DeactivateAccountService execution started. UserId: {UserId}", command.UserId);

        if (command.UserId == 0)
        {
            _logger.LogError("DeactivateAccount validation failed - User ID cannot be 0.");
            throw new ArgumentException("User ID cannot be 0.", nameof(command.UserId));
        }

        var account = await _deactivateAccount.FindAccountByIdAsync(command.UserId, cancellationToken);
        if (account == null)
        {
            _logger.LogWarning("Account not found. UserId: {UserId}", command.UserId);
            throw new AccountNotFound(command.UserId);
        }

        account.Deactivated = true;
        await _deactivateAccount.UpdateAccountAsync(account, cancellationToken);

        var model = DeactivateAccountModel.From(account, account.Deactivated);

        _logger.LogInformation("DeactivateAccountModel created successfully. UserId: {UserId}, Deactivated: {Deactivated}", command.UserId, account.Deactivated);

        return model;
    }
}
