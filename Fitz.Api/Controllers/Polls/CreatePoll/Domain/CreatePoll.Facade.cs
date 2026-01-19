namespace Fitz.Api.Controllers.Polls.CreatePoll.Domain;

public class CreatePollFacade(CreatePollService createPollService, ILogger<CreatePollFacade> logger)
{
    private readonly CreatePollService _createPollService = createPollService;
    private readonly ILogger<CreatePollFacade> _logger = logger;

    public async Task<CreatePollResponse> Execute(CreatePollCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("CreatePollFacade execution started. AccountId: {AccountId}", command.AccountId);

        var model = await _createPollService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("CreatePollService execution completed. PollId: {PollId}", model.Poll.Id);

        var response = CreatePollResponse.From(model);

        _logger.LogInformation("CreatePollFacade execution completed successfully. PollId: {PollId}", model.Poll.Id);

        return response;
    }
}
