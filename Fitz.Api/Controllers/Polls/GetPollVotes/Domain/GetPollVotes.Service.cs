using Fitz.Api.Controllers.Polls.Exceptions;
using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.GetPollVotes.Domain;

public class GetPollVotesService(IGetPollVotes getPollVotes, ILogger<GetPollVotesService> logger)
{
    private readonly IGetPollVotes _getPollVotes = getPollVotes;
    private readonly ILogger<GetPollVotesService> _logger = logger;

    public async Task<GetPollVotesModel> ExecuteAsync(GetPollVotesCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetPollVotesService execution started. PollId: {PollId}", command.PollId);

        var poll = await _getPollVotes.FindPollByIdAsync(command.PollId, cancellationToken);
        if (poll == null)
        {
            _logger.LogWarning("Poll not found. PollId: {PollId}", command.PollId);
            throw new PollNotFound(command.PollId);
        }

        var votes = await _getPollVotes.GetPollVotesAsync(command.PollId, cancellationToken);

        _logger.LogInformation("GetPollVotesService execution completed. PollId: {PollId}, VotesCount: {Count}", command.PollId, votes.Count);

        return GetPollVotesModel.From(poll, votes);
    }
}
