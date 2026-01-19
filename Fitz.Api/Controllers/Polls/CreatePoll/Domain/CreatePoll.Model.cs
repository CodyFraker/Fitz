using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.CreatePoll.Domain;

public record CreatePollModel(
    PollEntity Poll,
    List<PollOptionsEntity> Options)
{
    public static CreatePollModel From(PollEntity poll, List<PollOptionsEntity> options)
    {
        return new CreatePollModel(
            Poll: poll,
            Options: options
        );
    }
}
