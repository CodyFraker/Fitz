using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.UpdateVote.Domain;

public record UpdateVoteModel(
    Vote Vote)
{
    public static UpdateVoteModel From(Vote vote)
    {
        return new UpdateVoteModel(
            Vote: vote
        );
    }
}
