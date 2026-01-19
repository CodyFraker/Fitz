using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.GetPolls.Domain;

public class GetPollsService(IGetPolls getPolls, ILogger<GetPollsService> logger)
{
    private readonly IGetPolls _getPolls = getPolls;
    private readonly ILogger<GetPollsService> _logger = logger;

    public async Task<GetPollsModel> ExecuteAsync(GetPollsCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetPollsService execution started. Status: {Status}, UserId: {UserId}", command.Status, command.UserId);

        List<PollEntity> polls;

        if (command.UserId.HasValue)
        {
            polls = await _getPolls.GetPollsByUserIdAsync(command.UserId.Value, cancellationToken);
            if (command.Status.HasValue)
            {
                polls = polls.Where(p => p.Status == command.Status.Value).ToList();
            }
        }
        else
        {
            polls = await _getPolls.GetAllPollsAsync(cancellationToken);
            if (command.Status.HasValue)
            {
                polls = polls.Where(p => p.Status == command.Status.Value).ToList();
            }
        }

        _logger.LogInformation("GetPollsService execution completed. Count: {Count}", polls.Count);

        return GetPollsModel.From(polls);
    }
}
