using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.GetPoll.Domain;

public interface IGetPoll
{
    Task<PollEntity?> FindPollByIdAsync(int pollId, CancellationToken cancellationToken = default);
    Task<PollEntity?> FindPollByMessageIdAsync(ulong messageId, CancellationToken cancellationToken = default);
}
