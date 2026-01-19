using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.GetPollVotes.Domain;

public record GetPollVotesModel(
    PollEntity Poll,
    List<Vote> Votes)
{
    public static GetPollVotesModel From(PollEntity poll, List<Vote> votes)
    {
        return new GetPollVotesModel(
            Poll: poll,
            Votes: votes
        );
    }
}
