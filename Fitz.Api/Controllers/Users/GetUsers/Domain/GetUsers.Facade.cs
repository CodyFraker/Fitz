namespace Fitz.Api.Controllers.Users.GetUsers.Domain;

public class GetUsersFacade(GetUsersService getUsersService, ILogger<GetUsersFacade> logger)
{
    private readonly GetUsersService _getUsersService = getUsersService;
    private readonly ILogger<GetUsersFacade> _logger = logger;

    public async Task<GetUsersResponse> Execute(GetUsersCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetUsersFacade execution started. Query: {Query}, Page: {Page}, PageSize: {PageSize}", 
            command.Query, command.Page, command.PageSize);

        var model = await _getUsersService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("GetUsersService execution completed. TotalCount: {TotalCount}, Page: {Page}, PageSize: {PageSize}", 
            model.TotalCount, model.Page, model.PageSize);

        var response = GetUsersResponse.From(model);

        _logger.LogInformation("GetUsersFacade execution completed successfully. TotalCount: {TotalCount}, Page: {Page}, PageSize: {PageSize}", 
            model.TotalCount, model.Page, model.PageSize);

        return response;
    }
}
