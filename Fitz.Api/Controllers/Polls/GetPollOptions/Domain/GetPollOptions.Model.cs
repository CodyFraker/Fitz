using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.GetPollOptions.Domain;

public record GetPollOptionsModel(
    PollEntity Poll,
    List<PollOptionsEntity> Options)
{
    public static GetPollOptionsModel From(PollEntity poll, List<PollOptionsEntity> options)
    {
        return new GetPollOptionsModel(
            Poll: poll,
            Options: options
        );
    }
}
