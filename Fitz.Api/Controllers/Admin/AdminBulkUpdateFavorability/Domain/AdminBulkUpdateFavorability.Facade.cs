namespace Fitz.Api.Controllers.Admin.AdminBulkUpdateFavorability.Domain;

public class AdminBulkUpdateFavorabilityFacade(AdminBulkUpdateFavorabilityService adminBulkUpdateFavorabilityService, ILogger<AdminBulkUpdateFavorabilityFacade> logger)
{
    private readonly AdminBulkUpdateFavorabilityService _adminBulkUpdateFavorabilityService = adminBulkUpdateFavorabilityService;
    private readonly ILogger<AdminBulkUpdateFavorabilityFacade> _logger = logger;

    public async Task<AdminBulkUpdateFavorabilityResponse> Execute(AdminBulkUpdateFavorabilityCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AdminBulkUpdateFavorabilityFacade execution started. UserIdsCount: {UserIdsCount}, Favorability: {Favorability}", 
            command.UserIds?.Length ?? 0, command.Favorability);

        var model = await _adminBulkUpdateFavorabilityService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("AdminBulkUpdateFavorabilityService execution completed. SuccessCount: {SuccessCount}, FailCount: {FailCount}", 
            model.SuccessCount, model.FailCount);

        var response = AdminBulkUpdateFavorabilityResponse.From(model);

        _logger.LogInformation("AdminBulkUpdateFavorabilityFacade execution completed successfully. SuccessCount: {SuccessCount}, FailCount: {FailCount}", 
            model.SuccessCount, model.FailCount);

        return response;
    }
}
