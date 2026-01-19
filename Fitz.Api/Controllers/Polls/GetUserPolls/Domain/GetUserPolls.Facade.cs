namespace Fitz.Api.Controllers.Polls.GetUserPolls.Domain;

public class GetUserPollsFacade(GetUserPollsService getUserPollsService, ILogger<GetUserPollsFacade> logger)
{
    private readonly GetUserPollsService _getUserPollsService = getUserPollsService;
    private readonly ILogger<GetUserPollsFacade> _logger = logger;

    public async Task<GetUserPollsResponse> Execute(GetUserPollsCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetUserPollsFacade execution started. UserId: {UserId}", command.UserId);

        var model = await _getUserPollsService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("GetUserPollsService execution completed. UserId: {UserId}, Count: {Count}", command.UserId, model.Polls.Count);

        var response = GetUserPollsResponse.From(model);

        _logger.LogInformation("GetUserPollsFacade execution completed successfully. UserId: {UserId}, Count: {Count}", command.UserId, response.Polls.Count);

        return response;
    }
}
