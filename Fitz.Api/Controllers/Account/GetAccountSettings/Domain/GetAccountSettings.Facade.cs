namespace Fitz.Api.Controllers.Account.GetAccountSettings.Domain;

public class GetAccountSettingsFacade(GetAccountSettingsService getAccountSettingsService, ILogger<GetAccountSettingsFacade> logger)
{
    private readonly GetAccountSettingsService _getAccountSettingsService = getAccountSettingsService;
    private readonly ILogger<GetAccountSettingsFacade> _logger = logger;

    public async Task<GetAccountSettingsModel> Execute(GetAccountSettingsCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetAccountSettingsFacade execution started. UserId: {UserId}", command.UserId);

        var model = await _getAccountSettingsService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("GetAccountSettingsFacade execution completed successfully. UserId: {UserId}, Username: {Username}", command.UserId, model.Username);

        return model;
    }
}
