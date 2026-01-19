using Fitz.Api.Controllers.Polls.Exceptions;
using Fitz.Database.Entities;
using Fitz.Features.Bank;

namespace Fitz.Api.Controllers.Polls.EvaluatePoll.Domain;

public class EvaluatePollService(IEvaluatePoll evaluatePoll, BankService bankService, ILogger<EvaluatePollService> logger)
{
    private readonly IEvaluatePoll _evaluatePoll = evaluatePoll;
    private readonly BankService _bankService = bankService;
    private readonly ILogger<EvaluatePollService> _logger = logger;

    public async Task<EvaluatePollModel> ExecuteAsync(EvaluatePollCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("EvaluatePollService execution started. PollId: {PollId}, Status: {Status}", command.PollId, command.Status);

        var poll = await _evaluatePoll.FindPollByIdAsync(command.PollId, cancellationToken);
        if (poll == null)
        {
            _logger.LogWarning("Poll not found. PollId: {PollId}", command.PollId);
            throw new PollNotFound(command.PollId);
        }

        poll.Status = command.Status;
        poll.EvaluatedOn = DateTime.UtcNow;

        await _evaluatePoll.UpdatePollAsync(poll, cancellationToken);

        if (command.Status == PollStatusEnum.Approved)
        {
            await _bankService.AwardPollApproval(poll.AccountId);
        }

        _logger.LogInformation("EvaluatePollService execution completed. PollId: {PollId}, Status: {Status}", command.PollId, command.Status);

        return EvaluatePollModel.From(poll);
    }
}
