using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.GetUserPolls.Domain;

public interface IGetUserPolls
{
    Task<List<PollEntity>> GetPollsByUserIdAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<List<PollOptionsEntity>> GetPollOptionsByPollIdsAsync(List<int> pollIds, CancellationToken cancellationToken = default);
    Task<List<Vote>> GetVotesByPollIdsAsync(List<int> pollIds, CancellationToken cancellationToken = default);
}
