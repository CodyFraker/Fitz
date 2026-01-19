using Fitz.Api.Controllers.Polls.EvaluatePoll.Http;
using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.EvaluatePoll.Domain;

public record EvaluatePollCommand(
    int PollId,
    PollStatusEnum Status)
{
    public static EvaluatePollCommand From(int pollId, EvaluatePollRequestDto request)
    {
        return new EvaluatePollCommand(
            PollId: pollId,
            Status: request.Status
        );
    }
}
