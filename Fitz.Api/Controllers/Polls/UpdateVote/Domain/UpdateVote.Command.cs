using Fitz.Api.Controllers.Polls.UpdateVote.Http;

namespace Fitz.Api.Controllers.Polls.UpdateVote.Domain;

public record UpdateVoteCommand(
    int PollId,
    ulong UserId,
    int OptionId)
{
    public static UpdateVoteCommand From(int pollId, UpdateVoteRequestDto request)
    {
        return new UpdateVoteCommand(
            PollId: pollId,
            UserId: request.UserId,
            OptionId: request.OptionId
        );
    }
}
