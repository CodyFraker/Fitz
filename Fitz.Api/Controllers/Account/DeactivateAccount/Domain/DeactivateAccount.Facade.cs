namespace Fitz.Api.Controllers.Account.DeactivateAccount.Domain;

public class DeactivateAccountFacade(DeactivateAccountService deactivateAccountService, ILogger<DeactivateAccountFacade> logger)
{
    private readonly DeactivateAccountService _deactivateAccountService = deactivateAccountService;
    private readonly ILogger<DeactivateAccountFacade> _logger = logger;

    public async Task<DeactivateAccountResponse> Execute(DeactivateAccountCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("DeactivateAccountFacade execution started. UserId: {UserId}", command.UserId);

        var model = await _deactivateAccountService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("DeactivateAccountService execution completed. UserId: {UserId}, Deactivated: {Deactivated}", command.UserId, model.Deactivated);

        var response = DeactivateAccountResponse.From(model);

        _logger.LogInformation("DeactivateAccountFacade execution completed successfully. UserId: {UserId}, Deactivated: {Deactivated}", command.UserId, model.Deactivated);

        return response;
    }
}
