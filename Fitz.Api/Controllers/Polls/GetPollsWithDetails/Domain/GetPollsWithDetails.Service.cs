using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.GetPollsWithDetails.Domain;

public class GetPollsWithDetailsService(IGetPollsWithDetails getPollsWithDetails, ILogger<GetPollsWithDetailsService> logger)
{
    private readonly IGetPollsWithDetails _getPollsWithDetails = getPollsWithDetails;
    private readonly ILogger<GetPollsWithDetailsService> _logger = logger;

    public async Task<GetPollsWithDetailsModel> ExecuteAsync(GetPollsWithDetailsCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetPollsWithDetailsService execution started. Status: {Status}, UserId: {UserId}, Skip: {Skip}, Take: {Take}", 
            command.Status, command.UserId, command.Skip, command.Take);

        var polls = await _getPollsWithDetails.GetPollsAsync(command.Status, command.UserId, cancellationToken);
        var pollIds = polls.Select(p => p.Id).ToList();

        var options = await _getPollsWithDetails.GetPollOptionsByPollIdsAsync(pollIds, cancellationToken);
        var votes = await _getPollsWithDetails.GetVotesByPollIdsAsync(pollIds, cancellationToken);

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

        var sortedPolls = SortPolls(pollsWithDetails, command.SortBy, command.SortOrder);
        var totalCount = sortedPolls.Count;
        var paginatedPolls = sortedPolls.Skip(command.Skip).Take(command.Take).ToList();

        _logger.LogInformation("GetPollsWithDetailsService execution completed. TotalCount: {TotalCount}, Returned: {Returned}", 
            totalCount, paginatedPolls.Count);

        return GetPollsWithDetailsModel.From(paginatedPolls, totalCount, command.Skip, command.Take);
    }

    private List<PollWithDetails> SortPolls(List<PollWithDetails> polls, string sortBy, string sortOrder)
    {
        var isAscending = sortOrder.ToLower() == "asc";

        return sortBy.ToLower() switch
        {
            "totalvotes" => isAscending
                ? polls.OrderBy(p => p.TotalVotes).ToList()
                : polls.OrderByDescending(p => p.TotalVotes).ToList(),
            "submittedon" => isAscending
                ? polls.OrderBy(p => p.Poll.SubmittedOn).ToList()
                : polls.OrderByDescending(p => p.Poll.SubmittedOn).ToList(),
            "question" => isAscending
                ? polls.OrderBy(p => p.Poll.Question).ToList()
                : polls.OrderByDescending(p => p.Poll.Question).ToList(),
            _ => polls.OrderByDescending(p => p.TotalVotes).ToList()
        };
    }
}
