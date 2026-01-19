namespace Fitz.Api.Controllers.Polls.GetPolls.Domain;

public class GetPollsFacade(GetPollsService getPollsService, ILogger<GetPollsFacade> logger)
{
    private readonly GetPollsService _getPollsService = getPollsService;
    private readonly ILogger<GetPollsFacade> _logger = logger;

    public async Task<GetPollsResponse> Execute(GetPollsCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetPollsFacade execution started");

        var model = await _getPollsService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("GetPollsService execution completed. Count: {Count}", model.Polls.Count);

        var response = GetPollsResponse.From(model);

        _logger.LogInformation("GetPollsFacade execution completed successfully. Count: {Count}", response.Polls.Count);

        return response;
    }
}
