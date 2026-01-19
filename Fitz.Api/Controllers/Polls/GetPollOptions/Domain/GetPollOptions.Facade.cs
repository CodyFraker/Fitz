namespace Fitz.Api.Controllers.Polls.GetPollOptions.Domain;

public class GetPollOptionsFacade(GetPollOptionsService getPollOptionsService, ILogger<GetPollOptionsFacade> logger)
{
    private readonly GetPollOptionsService _getPollOptionsService = getPollOptionsService;
    private readonly ILogger<GetPollOptionsFacade> _logger = logger;

    public async Task<GetPollOptionsResponse> Execute(GetPollOptionsCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetPollOptionsFacade execution started. PollId: {PollId}", command.PollId);

        var model = await _getPollOptionsService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("GetPollOptionsService execution completed. PollId: {PollId}, OptionsCount: {Count}", command.PollId, model.Options.Count);

        var response = GetPollOptionsResponse.From(model);

        _logger.LogInformation("GetPollOptionsFacade execution completed successfully. PollId: {PollId}, OptionsCount: {Count}", command.PollId, response.Options.Count);

        return response;
    }
}
