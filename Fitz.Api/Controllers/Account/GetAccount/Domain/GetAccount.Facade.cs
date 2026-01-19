namespace Fitz.Api.Controllers.Account.GetAccount.Domain;

public class GetAccountFacade(GetAccountService getAccountService, ILogger<GetAccountFacade> logger)
{
    private readonly GetAccountService _getAccountService = getAccountService;
    private readonly ILogger<GetAccountFacade> _logger = logger;

    public async Task<GetAccountResponse> Execute(GetAccountCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetAccountFacade execution started. UserId: {UserId}", command.UserId);

        var model = await _getAccountService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("GetAccountService execution completed. UserId: {UserId}", command.UserId);

        var response = GetAccountResponse.From(model);

        _logger.LogInformation("GetAccountFacade execution completed successfully. UserId: {UserId}, Username: {Username}", command.UserId, model.Username);

        return response;
    }
}
