using Fitz.Api.Controllers.Polls.Exceptions;
using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.GetPoll.Domain;

public class GetPollService(IGetPoll getPoll, ILogger<GetPollService> logger)
{
    private readonly IGetPoll _getPoll = getPoll;
    private readonly ILogger<GetPollService> _logger = logger;

    public async Task<GetPollModel> ExecuteAsync(GetPollCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetPollService execution started. PollId: {PollId}, MessageId: {MessageId}", command.PollId, command.MessageId);

        PollEntity? poll;

        if (command.PollId.HasValue)
        {
            poll = await _getPoll.FindPollByIdAsync(command.PollId.Value, cancellationToken);
        }
        else if (command.MessageId.HasValue)
        {
            poll = await _getPoll.FindPollByMessageIdAsync(command.MessageId.Value, cancellationToken);
        }
        else
        {
            _logger.LogError("GetPollCommand must have either PollId or MessageId");
            throw new ArgumentException("Either PollId or MessageId must be provided");
        }

        if (poll == null)
        {
            if (command.PollId.HasValue)
            {
                _logger.LogWarning("Poll not found. PollId: {PollId}", command.PollId.Value);
                throw new PollNotFound(command.PollId.Value);
            }
            else
            {
                _logger.LogWarning("Poll not found. MessageId: {MessageId}", command.MessageId!.Value);
                throw new PollNotFound(command.MessageId.Value);
            }
        }

        _logger.LogInformation("GetPollService execution completed. PollId: {PollId}", poll.Id);

        return GetPollModel.From(poll);
    }
}
