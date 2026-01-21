using Fitz.Api.Controllers.Account.Exceptions;

namespace Fitz.Api.Controllers.Bank.GetBalance.Domain;

public class GetBalanceService(IGetBalance getBalance, ILogger<GetBalanceService> logger)
{
    private readonly IGetBalance _getBalance = getBalance;
    private readonly ILogger<GetBalanceService> _logger = logger;

    public async Task<GetBalanceModel> ExecuteAsync(GetBalanceCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetBalanceService execution started. UserId: {UserId}", command.UserId);

        if (command.UserId == 0)
        {
            _logger.LogError("GetBalance validation failed - User ID cannot be 0.");
            throw new ArgumentException("User ID cannot be 0.", nameof(command.UserId));
        }

        var account = await _getBalance.FindAccountByIdAsync(command.UserId, cancellationToken);
        
        if (account == null)
        {
            _logger.LogInformation("Account not found for user {UserId}, creating new account", command.UserId);
            
            var username = command.Username ?? $"User_{command.UserId}";
            account = await _getBalance.CreateAccountAsync(command.UserId, username, cancellationToken);
            
            _logger.LogInformation("Account created successfully. UserId: {UserId}, Username: {Username}", command.UserId, username);
        }

        var model = GetBalanceModel.From(account);

        _logger.LogInformation("GetBalanceModel created successfully. UserId: {UserId}, Beer: {Beer}", command.UserId, model.Account.Beer);

        return model;
    }
}
