using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.PostPollToPending.Domain;

public interface IPostPollToPending
{
    Task<PollEntity?> FindPollByIdAsync(int pollId, CancellationToken cancellationToken = default);
    Task<List<PollOptionsEntity>> GetPollOptionsAsync(int pollId, CancellationToken cancellationToken = default);
    Task UpdatePollAsync(PollEntity poll, CancellationToken cancellationToken = default);
}
