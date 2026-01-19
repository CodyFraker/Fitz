using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.GetPolls.Domain;

public record GetPollsModel(
    List<PollEntity> Polls)
{
    public static GetPollsModel From(List<PollEntity> polls)
    {
        return new GetPollsModel(
            Polls: polls
        );
    }
}
