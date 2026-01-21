namespace Fitz.Api.Controllers.Admin.AdminUpdateFavorability.Domain;

public class AdminUpdateFavorabilityFacade(AdminUpdateFavorabilityService adminUpdateFavorabilityService, ILogger<AdminUpdateFavorabilityFacade> logger)
{
    private readonly AdminUpdateFavorabilityService _adminUpdateFavorabilityService = adminUpdateFavorabilityService;
    private readonly ILogger<AdminUpdateFavorabilityFacade> _logger = logger;

    public async Task<AdminUpdateFavorabilityResponse> Execute(AdminUpdateFavorabilityCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AdminUpdateFavorabilityFacade execution started. UserId: {UserId}, Favorability: {Favorability}", 
            command.UserId, command.Favorability);

        var model = await _adminUpdateFavorabilityService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("AdminUpdateFavorabilityService execution completed. Message: {Message}", model.Message);

        var response = AdminUpdateFavorabilityResponse.From(model);

        _logger.LogInformation("AdminUpdateFavorabilityFacade execution completed successfully. Message: {Message}", model.Message);

        return response;
    }
}
