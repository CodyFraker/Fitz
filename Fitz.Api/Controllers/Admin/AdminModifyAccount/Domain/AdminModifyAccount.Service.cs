using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Features.Accounts;

namespace Fitz.Api.Controllers.Admin.AdminModifyAccount.Domain;

public class AdminModifyAccountService(
    IAdminModifyAccount adminModifyAccount,
    AccountService accountService,
    ILogger<AdminModifyAccountService> logger)
{
    private readonly IAdminModifyAccount _adminModifyAccount = adminModifyAccount;
    private readonly AccountService _accountService = accountService;
    private readonly ILogger<AdminModifyAccountService> _logger = logger;

    public async Task<AdminModifyAccountModel> ExecuteAsync(AdminModifyAccountCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AdminModifyAccountService execution started. UserId: {UserId}", command.UserId);

        if (command.UserId == 0)
        {
            _logger.LogError("AdminModifyAccount validation failed - User ID cannot be 0.");
            throw new ArgumentException("User ID cannot be 0.", nameof(command.UserId));
        }

        var account = await _adminModifyAccount.FindAccountByIdAsync(command.UserId, cancellationToken);
        if (account == null)
        {
            _logger.LogWarning("Account not found. UserId: {UserId}", command.UserId);
            throw new AccountNotFound(command.UserId);
        }

        bool hasChanges = false;

        if (command.Beer.HasValue)
        {
            account.Beer = command.Beer.Value;
            await _adminModifyAccount.UpdateAccountAsync(account, cancellationToken);
            hasChanges = true;
        }

        if (command.LifetimeBeer.HasValue)
        {
            account.LifetimeBeer = command.LifetimeBeer.Value;
            await _adminModifyAccount.UpdateAccountAsync(account, cancellationToken);
            hasChanges = true;
        }

        if (command.SafeBalance.HasValue)
        {
            var result = await _accountService.SetSafeBalanceAsync(account, command.SafeBalance.Value);
            if (!result.Success)
            {
                _logger.LogError("Failed to set safe balance. Message: {Message}", result.Message);
                throw new InvalidOperationException(result.Message);
            }
            hasChanges = true;
        }

        if (command.Favorability.HasValue)
        {
            var result = await _accountService.SetFavorabilityAsync(account, command.Favorability.Value);
            if (!result.Success)
            {
                _logger.LogError("Failed to set favorability. Message: {Message}", result.Message);
                throw new InvalidOperationException(result.Message);
            }
            hasChanges = true;
        }

        if (command.SubscribeToLottery.HasValue)
        {
            var result = await _accountService.SetLotterySubscribe(account, command.SubscribeToLottery.Value);
            if (!result.Success)
            {
                _logger.LogError("Failed to set lottery subscribe. Message: {Message}", result.Message);
                throw new InvalidOperationException(result.Message);
            }
            hasChanges = true;
        }

        if (command.SubscribeTickets.HasValue)
        {
            var result = await _accountService.SetTicketAmountAsync(account, command.SubscribeTickets.Value);
            if (!result.Success)
            {
                _logger.LogError("Failed to set ticket amount. Message: {Message}", result.Message);
                throw new InvalidOperationException(result.Message);
            }
            hasChanges = true;
        }

        if (command.Deactivated.HasValue)
        {
            var result = await _accountService.SetDeactivatedAsync(account, command.Deactivated.Value);
            if (!result.Success)
            {
                _logger.LogError("Failed to set deactivated. Message: {Message}", result.Message);
                throw new InvalidOperationException(result.Message);
            }
            hasChanges = true;
        }

        if (!hasChanges)
        {
            _logger.LogWarning("No changes to apply to account. UserId: {UserId}", command.UserId);
            throw new ArgumentException("At least one field must be provided to update.");
        }

        var updatedAccount = await _adminModifyAccount.GetAccountAfterUpdateAsync(command.UserId, cancellationToken);
        if (updatedAccount == null)
        {
            _logger.LogError("Account not found after update. UserId: {UserId}", command.UserId);
            throw new AccountNotFound(command.UserId);
        }

        var model = AdminModifyAccountModel.From(updatedAccount);

        _logger.LogInformation("AdminModifyAccountModel created successfully. UserId: {UserId}", command.UserId);

        return model;
    }
}
