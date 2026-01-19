using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.GetPollsWithDetails.Domain;

public record GetPollsWithDetailsModel(
    List<PollWithDetails> Polls,
    int TotalCount,
    int Skip,
    int Take)
{
    public static GetPollsWithDetailsModel From(
        List<PollWithDetails> polls,
        int totalCount,
        int skip,
        int take)
    {
        return new GetPollsWithDetailsModel(
            Polls: polls,
            TotalCount: totalCount,
            Skip: skip,
            Take: take
        );
    }
}

public record PollWithDetails(
    PollEntity Poll,
    List<PollOptionsEntity> Options,
    List<Vote> Votes,
    Dictionary<int, int> OptionVoteCounts,
    int TotalVotes);
