namespace Fitz.Api.Controllers.Polls.AddVote.Domain;

public class AddVoteFacade(AddVoteService addVoteService, ILogger<AddVoteFacade> logger)
{
    private readonly AddVoteService _addVoteService = addVoteService;
    private readonly ILogger<AddVoteFacade> _logger = logger;

    public async Task<AddVoteResponse> Execute(AddVoteCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AddVoteFacade execution started. PollId: {PollId}, UserId: {UserId}", command.PollId, command.UserId);

        var model = await _addVoteService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("AddVoteService execution completed. PollId: {PollId}, UserId: {UserId}", command.PollId, command.UserId);

        var response = AddVoteResponse.From(model);

        _logger.LogInformation("AddVoteFacade execution completed successfully. PollId: {PollId}, UserId: {UserId}", command.PollId, command.UserId);

        return response;
    }
}
