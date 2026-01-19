using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.GetUserPolls.Domain;

public record GetUserPollsModel(
    List<PollWithDetails> Polls)
{
    public static GetUserPollsModel From(List<PollWithDetails> polls)
    {
        return new GetUserPollsModel(
            Polls: polls
        );
    }
}

public record PollWithDetails(
    PollEntity Poll,
    List<PollOptionsEntity> Options,
    List<Vote> Votes,
    Dictionary<int, int> OptionVoteCounts,
    int TotalVotes);
