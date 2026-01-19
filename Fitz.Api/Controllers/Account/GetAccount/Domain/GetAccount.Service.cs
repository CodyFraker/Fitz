using Fitz.Api.Controllers.Account.Exceptions;

namespace Fitz.Api.Controllers.Account.GetAccount.Domain;

public class GetAccountService(IGetAccount getAccount, ILogger<GetAccountService> logger)
{
    private readonly IGetAccount _getAccount = getAccount;
    private readonly ILogger<GetAccountService> _logger = logger;

    public async Task<GetAccountModel> ExecuteAsync(GetAccountCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetAccountService execution started. UserId: {UserId}", command.UserId);

        if (command.UserId == 0)
        {
            _logger.LogError("GetAccount validation failed - User ID cannot be 0.");
            throw new ArgumentException("User ID cannot be 0.", nameof(command.UserId));
        }

        var accountEntity = await _getAccount.FindByIdAsync(command.UserId, cancellationToken);
        if (accountEntity == null)
        {
            _logger.LogWarning("Account not found. UserId: {UserId}", command.UserId);
            throw new AccountNotFound(command.UserId);
        }

        var model = GetAccountModel.From(accountEntity);

        _logger.LogInformation("GetAccountModel created successfully. UserId: {UserId}, Username: {Username}", command.UserId, model.Username);

        return model;
    }
}
