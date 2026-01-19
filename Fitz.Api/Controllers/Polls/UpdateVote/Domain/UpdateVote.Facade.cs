namespace Fitz.Api.Controllers.Polls.UpdateVote.Domain;

public class UpdateVoteFacade(UpdateVoteService updateVoteService, ILogger<UpdateVoteFacade> logger)
{
    private readonly UpdateVoteService _updateVoteService = updateVoteService;
    private readonly ILogger<UpdateVoteFacade> _logger = logger;

    public async Task<UpdateVoteResponse> Execute(UpdateVoteCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("UpdateVoteFacade execution started. PollId: {PollId}, UserId: {UserId}", command.PollId, command.UserId);

        var model = await _updateVoteService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("UpdateVoteService execution completed. PollId: {PollId}, UserId: {UserId}", command.PollId, command.UserId);

        var response = UpdateVoteResponse.From(model);

        _logger.LogInformation("UpdateVoteFacade execution completed successfully. PollId: {PollId}, UserId: {UserId}", command.PollId, command.UserId);

        return response;
    }
}
