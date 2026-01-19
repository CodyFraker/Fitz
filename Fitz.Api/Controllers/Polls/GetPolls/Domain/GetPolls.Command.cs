using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.GetPolls.Domain;

public record GetPollsCommand(
    PollStatusEnum? Status,
    ulong? UserId)
{
    public static GetPollsCommand From(PollStatusEnum? status, ulong? userId)
    {
        return new GetPollsCommand(
            Status: status,
            UserId: userId
        );
    }
}
