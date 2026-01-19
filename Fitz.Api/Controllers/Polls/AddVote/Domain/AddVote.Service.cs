using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Polls.Exceptions;
using Fitz.Database.Entities;
using Fitz.Features.Bank;

namespace Fitz.Api.Controllers.Polls.AddVote.Domain;

public class AddVoteService(IAddVote addVote, BankService bankService, ILogger<AddVoteService> logger)
{
    private readonly IAddVote _addVote = addVote;
    private readonly BankService _bankService = bankService;
    private readonly ILogger<AddVoteService> _logger = logger;

    public async Task<AddVoteModel> ExecuteAsync(AddVoteCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AddVoteService execution started. PollId: {PollId}, UserId: {UserId}, OptionId: {OptionId}", 
            command.PollId, command.UserId, command.OptionId);

        var poll = await _addVote.FindPollByIdAsync(command.PollId, cancellationToken);
        if (poll == null)
        {
            _logger.LogWarning("Poll not found. PollId: {PollId}", command.PollId);
            throw new PollNotFound(command.PollId);
        }

        var option = await _addVote.FindPollOptionAsync(command.PollId, command.OptionId, cancellationToken);
        if (option == null)
        {
            _logger.LogWarning("Poll option not found. PollId: {PollId}, OptionId: {OptionId}", command.PollId, command.OptionId);
            throw new PollOptionNotFound(command.OptionId);
        }

        var account = await _addVote.FindAccountByIdAsync(command.UserId, cancellationToken);
        if (account == null)
        {
            _logger.LogWarning("Account not found. UserId: {UserId}", command.UserId);
            throw new AccountNotFound(command.UserId);
        }

        var existingVote = await _addVote.FindVoteAsync(command.PollId, command.UserId, cancellationToken);
        if (existingVote != null)
        {
            _logger.LogInformation("User has already voted on this poll. PollId: {PollId}, UserId: {UserId}", command.PollId, command.UserId);
            return AddVoteModel.From();
        }

        var vote = new Vote
        {
            PollId = command.PollId,
            Choice = command.OptionId,
            UserId = command.UserId,
            Timestamp = DateTime.UtcNow
        };

        await _addVote.CreateVoteAsync(vote, cancellationToken);

        await _bankService.AwardPollVote(command.UserId);

        if (poll.AccountId != command.UserId)
        {
            await _bankService.TipPollCreatorVote(poll.AccountId);
        }

        _logger.LogInformation("AddVoteService execution completed. PollId: {PollId}, UserId: {UserId}", command.PollId, command.UserId);

        return AddVoteModel.From();
    }
}
