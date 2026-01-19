using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.GetUserPolls.Domain;

public class GetUserPollsService(IGetUserPolls getUserPolls, ILogger<GetUserPollsService> logger)
{
    private readonly IGetUserPolls _getUserPolls = getUserPolls;
    private readonly ILogger<GetUserPollsService> _logger = logger;

    public async Task<GetUserPollsModel> ExecuteAsync(GetUserPollsCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetUserPollsService execution started. UserId: {UserId}", command.UserId);

        var polls = await _getUserPolls.GetPollsByUserIdAsync(command.UserId, cancellationToken);
        var pollIds = polls.Select(p => p.Id).ToList();

        var options = await _getUserPolls.GetPollOptionsByPollIdsAsync(pollIds, cancellationToken);
        var votes = await _getUserPolls.GetVotesByPollIdsAsync(pollIds, cancellationToken);

        var totalVotesByPoll = votes
            .GroupBy(v => v.PollId)
            .ToDictionary(g => g.Key, g => g.Count());

        var optionVoteCountsByPoll = votes
            .Where(v => v.Choice.HasValue)
            .GroupBy(v => new { v.PollId, OptionId = v.Choice.Value })
            .ToDictionary(g => (g.Key.PollId, g.Key.OptionId), g => g.Count());

        var pollsWithDetails = polls.Select(p =>
        {
            var pollOptions = options.Where(o => o.PollId == p.Id).ToList();
            var pollVoteCounts = pollOptions.ToDictionary(
                o => o.Id,
                o => optionVoteCountsByPoll.TryGetValue((p.Id, o.Id), out var count) ? count : 0
            );

            return new PollWithDetails(
                Poll: p,
                Options: pollOptions,
                Votes: votes.Where(v => v.PollId == p.Id).ToList(),
                OptionVoteCounts: pollVoteCounts,
                TotalVotes: totalVotesByPoll.GetValueOrDefault(p.Id, 0)
            );
        }).ToList();

        _logger.LogInformation("GetUserPollsService execution completed. UserId: {UserId}, Count: {Count}", command.UserId, pollsWithDetails.Count);

        return GetUserPollsModel.From(pollsWithDetails);
    }
}
