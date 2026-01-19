using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.EvaluatePoll.Domain;

public record EvaluatePollModel(
    PollEntity Poll)
{
    public static EvaluatePollModel From(PollEntity poll)
    {
        return new EvaluatePollModel(
            Poll: poll
        );
    }
}
