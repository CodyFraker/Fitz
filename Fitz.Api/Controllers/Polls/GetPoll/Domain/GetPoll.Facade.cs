namespace Fitz.Api.Controllers.Polls.GetPoll.Domain;

public class GetPollFacade(GetPollService getPollService, ILogger<GetPollFacade> logger)
{
    private readonly GetPollService _getPollService = getPollService;
    private readonly ILogger<GetPollFacade> _logger = logger;

    public async Task<GetPollResponse> Execute(GetPollCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetPollFacade execution started");

        var model = await _getPollService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("GetPollService execution completed. PollId: {PollId}", model.Poll.Id);

        var response = GetPollResponse.From(model);

        _logger.LogInformation("GetPollFacade execution completed successfully. PollId: {PollId}", model.Poll.Id);

        return response;
    }
}
