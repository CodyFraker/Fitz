using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.PostPollToPending.Domain;

public record PostPollToPendingModel(
    PollEntity Poll)
{
    public static PostPollToPendingModel From(PollEntity poll)
    {
        return new PostPollToPendingModel(
            Poll: poll
        );
    }
}
