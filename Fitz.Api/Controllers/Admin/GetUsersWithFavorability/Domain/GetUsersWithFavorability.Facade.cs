namespace Fitz.Api.Controllers.Admin.GetUsersWithFavorability.Domain;

public class GetUsersWithFavorabilityFacade(GetUsersWithFavorabilityService getUsersWithFavorabilityService, ILogger<GetUsersWithFavorabilityFacade> logger)
{
    private readonly GetUsersWithFavorabilityService _getUsersWithFavorabilityService = getUsersWithFavorabilityService;
    private readonly ILogger<GetUsersWithFavorabilityFacade> _logger = logger;

    public async Task<GetUsersWithFavorabilityResponse> Execute(GetUsersWithFavorabilityCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetUsersWithFavorabilityFacade execution started. Query: {Query}, Skip: {Skip}, Take: {Take}", 
            command.Query, command.Skip, command.Take);

        var model = await _getUsersWithFavorabilityService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("GetUsersWithFavorabilityService execution completed. TotalCount: {TotalCount}, Returned: {Returned}", 
            model.TotalCount, model.Accounts.Count);

        var response = GetUsersWithFavorabilityResponse.From(model);

        _logger.LogInformation("GetUsersWithFavorabilityFacade execution completed successfully. TotalCount: {TotalCount}, Returned: {Returned}", 
            model.TotalCount, model.Accounts.Count);

        return response;
    }
}
