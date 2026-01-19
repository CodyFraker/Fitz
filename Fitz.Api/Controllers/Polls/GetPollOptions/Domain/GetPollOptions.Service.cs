using Fitz.Api.Controllers.Polls.Exceptions;
using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.GetPollOptions.Domain;

public class GetPollOptionsService(IGetPollOptions getPollOptions, ILogger<GetPollOptionsService> logger)
{
    private readonly IGetPollOptions _getPollOptions = getPollOptions;
    private readonly ILogger<GetPollOptionsService> _logger = logger;

    public async Task<GetPollOptionsModel> ExecuteAsync(GetPollOptionsCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetPollOptionsService execution started. PollId: {PollId}", command.PollId);

        var poll = await _getPollOptions.FindPollByIdAsync(command.PollId, cancellationToken);
        if (poll == null)
        {
            _logger.LogWarning("Poll not found. PollId: {PollId}", command.PollId);
            throw new PollNotFound(command.PollId);
        }

        var options = await _getPollOptions.GetPollOptionsAsync(command.PollId, cancellationToken);

        _logger.LogInformation("GetPollOptionsService execution completed. PollId: {PollId}, OptionsCount: {Count}", command.PollId, options.Count);

        return GetPollOptionsModel.From(poll, options);
    }
}
