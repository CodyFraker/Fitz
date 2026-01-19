namespace Fitz.Api.Controllers.Polls.GetPollVotes.Domain;

public class GetPollVotesFacade(GetPollVotesService getPollVotesService, ILogger<GetPollVotesFacade> logger)
{
    private readonly GetPollVotesService _getPollVotesService = getPollVotesService;
    private readonly ILogger<GetPollVotesFacade> _logger = logger;

    public async Task<GetPollVotesResponse> Execute(GetPollVotesCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetPollVotesFacade execution started. PollId: {PollId}", command.PollId);

        var model = await _getPollVotesService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("GetPollVotesService execution completed. PollId: {PollId}, VotesCount: {Count}", command.PollId, model.Votes.Count);

        var response = GetPollVotesResponse.From(model);

        _logger.LogInformation("GetPollVotesFacade execution completed successfully. PollId: {PollId}, VotesCount: {Count}", command.PollId, response.Votes.Count);

        return response;
    }
}
