namespace Fitz.Api.Controllers.Polls.EvaluatePoll.Domain;

public class EvaluatePollFacade(EvaluatePollService evaluatePollService, ILogger<EvaluatePollFacade> logger)
{
    private readonly EvaluatePollService _evaluatePollService = evaluatePollService;
    private readonly ILogger<EvaluatePollFacade> _logger = logger;

    public async Task<EvaluatePollResponse> Execute(EvaluatePollCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("EvaluatePollFacade execution started. PollId: {PollId}, Status: {Status}", command.PollId, command.Status);

        var model = await _evaluatePollService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("EvaluatePollService execution completed. PollId: {PollId}, Status: {Status}", command.PollId, command.Status);

        var response = EvaluatePollResponse.From(model);

        _logger.LogInformation("EvaluatePollFacade execution completed successfully. PollId: {PollId}, Status: {Status}", command.PollId, command.Status);

        return response;
    }
}
