namespace Fitz.Api.Controllers.Admin.AdminDeletePoll.Domain;

public class AdminDeletePollFacade(AdminDeletePollService adminDeletePollService, ILogger<AdminDeletePollFacade> logger)
{
    private readonly AdminDeletePollService _adminDeletePollService = adminDeletePollService;
    private readonly ILogger<AdminDeletePollFacade> _logger = logger;

    public async Task<AdminDeletePollResponse> Execute(AdminDeletePollCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AdminDeletePollFacade execution started. Id: {Id}", command.Id);

        var model = await _adminDeletePollService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("AdminDeletePollService execution completed. Id: {Id}, Message: {Message}", command.Id, model.Message);

        var response = AdminDeletePollResponse.From(model);

        _logger.LogInformation("AdminDeletePollFacade execution completed successfully. Id: {Id}, Message: {Message}", command.Id, model.Message);

        return response;
    }
}
