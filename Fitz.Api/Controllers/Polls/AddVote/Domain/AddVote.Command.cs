using Fitz.Api.Controllers.Polls.AddVote.Http;

namespace Fitz.Api.Controllers.Polls.AddVote.Domain;

public record AddVoteCommand(
    int PollId,
    ulong UserId,
    int OptionId)
{
    public static AddVoteCommand From(int pollId, AddVoteRequestDto request)
    {
        return new AddVoteCommand(
            PollId: pollId,
            UserId: request.UserId,
            OptionId: request.OptionId
        );
    }
}
