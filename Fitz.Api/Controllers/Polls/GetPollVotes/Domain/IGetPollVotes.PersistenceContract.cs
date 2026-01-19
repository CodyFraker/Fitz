using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.GetPollVotes.Domain;

public interface IGetPollVotes
{
    Task<PollEntity?> FindPollByIdAsync(int pollId, CancellationToken cancellationToken = default);
    Task<List<Vote>> GetPollVotesAsync(int pollId, CancellationToken cancellationToken = default);
}
