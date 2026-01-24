using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Account.GetAccount.Domain;

namespace Fitz.Api.Controllers.Account.GetAccountSettings.Domain;

public class GetAccountSettingsService(GetAccountService getAccountService, ILogger<GetAccountSettingsService> logger)
{
    private readonly GetAccountService _getAccountService = getAccountService;
    private readonly ILogger<GetAccountSettingsService> _logger = logger;

    public async Task<GetAccountSettingsModel> ExecuteAsync(GetAccountSettingsCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetAccountSettingsService execution started. UserId: {UserId}", command.UserId);

        if (command.UserId == 0)
        {
            _logger.LogError("GetAccountSettings validation failed - User ID cannot be 0.");
            throw new ArgumentException("User ID cannot be 0.", nameof(command.UserId));
        }

        var getAccountCommand = GetAccountCommand.From(command.UserId);
        var getAccountModel = await _getAccountService.ExecuteAsync(getAccountCommand, cancellationToken);

        var model = GetAccountSettingsModel.From(getAccountModel);

        _logger.LogInformation("GetAccountSettingsModel created successfully. UserId: {UserId}, Username: {Username}", command.UserId, model.Username);

        return model;
    }
}
