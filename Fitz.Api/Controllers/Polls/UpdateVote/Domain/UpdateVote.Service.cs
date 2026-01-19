using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Polls.Exceptions;
using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.UpdateVote.Domain;

public class UpdateVoteService(IUpdateVote updateVote, ILogger<UpdateVoteService> logger)
{
    private readonly IUpdateVote _updateVote = updateVote;
    private readonly ILogger<UpdateVoteService> _logger = logger;

    public async Task<UpdateVoteModel> ExecuteAsync(UpdateVoteCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("UpdateVoteService execution started. PollId: {PollId}, UserId: {UserId}, OptionId: {OptionId}", 
            command.PollId, command.UserId, command.OptionId);

        var poll = await _updateVote.FindPollByIdAsync(command.PollId, cancellationToken);
        if (poll == null)
        {
            _logger.LogWarning("Poll not found. PollId: {PollId}", command.PollId);
            throw new PollNotFound(command.PollId);
        }

        var vote = await _updateVote.FindVoteAsync(command.PollId, command.UserId, cancellationToken);
        if (vote == null)
        {
            _logger.LogWarning("Vote not found. PollId: {PollId}, UserId: {UserId}", command.PollId, command.UserId);
            throw new VoteNotFound(command.PollId, command.UserId);
        }

        var account = await _updateVote.FindAccountByIdAsync(command.UserId, cancellationToken);
        if (account == null)
        {
            _logger.LogWarning("Account not found. UserId: {UserId}", command.UserId);
            throw new AccountNotFound(command.UserId);
        }

        vote.Choice = command.OptionId;
        vote.Timestamp = DateTime.UtcNow;

        await _updateVote.UpdateVoteAsync(vote, cancellationToken);

        _logger.LogInformation("UpdateVoteService execution completed. PollId: {PollId}, UserId: {UserId}", command.PollId, command.UserId);

        return UpdateVoteModel.From(vote);
    }
}
