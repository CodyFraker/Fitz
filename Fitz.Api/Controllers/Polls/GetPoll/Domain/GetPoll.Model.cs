using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.GetPoll.Domain;

public record GetPollModel(
    PollEntity Poll)
{
    public static GetPollModel From(PollEntity poll)
    {
        return new GetPollModel(
            Poll: poll
        );
    }
}
