using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.UpdateVote.Domain;

public record UpdateVoteResponse(
    int Id,
    int PollId,
    int? Choice,
    ulong UserId,
    DateTime Timestamp)
{
    public static UpdateVoteResponse From(UpdateVoteModel model)
    {
        return new UpdateVoteResponse(
            Id: model.Vote.Id,
            PollId: model.Vote.PollId,
            Choice: model.Vote.Choice,
            UserId: model.Vote.UserId,
            Timestamp: model.Vote.Timestamp
        );
    }
}
