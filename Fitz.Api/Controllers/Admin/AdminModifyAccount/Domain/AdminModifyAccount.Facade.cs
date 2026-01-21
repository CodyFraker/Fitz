namespace Fitz.Api.Controllers.Admin.AdminModifyAccount.Domain;

public class AdminModifyAccountFacade(AdminModifyAccountService adminModifyAccountService, ILogger<AdminModifyAccountFacade> logger)
{
    private readonly AdminModifyAccountService _adminModifyAccountService = adminModifyAccountService;
    private readonly ILogger<AdminModifyAccountFacade> _logger = logger;

    public async Task<AdminModifyAccountResponse> Execute(AdminModifyAccountCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AdminModifyAccountFacade execution started. UserId: {UserId}", command.UserId);

        var model = await _adminModifyAccountService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("AdminModifyAccountService execution completed. UserId: {UserId}", command.UserId);

        var response = AdminModifyAccountResponse.From(model);

        _logger.LogInformation("AdminModifyAccountFacade execution completed successfully. UserId: {UserId}", command.UserId);

        return response;
    }
}
