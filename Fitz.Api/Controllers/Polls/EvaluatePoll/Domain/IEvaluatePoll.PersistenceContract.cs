using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.EvaluatePoll.Domain;

public interface IEvaluatePoll
{
    Task<PollEntity?> FindPollByIdAsync(int pollId, CancellationToken cancellationToken = default);
    Task UpdatePollAsync(PollEntity poll, CancellationToken cancellationToken = default);
}
