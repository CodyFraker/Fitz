using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.GetPollsWithDetails.Domain;

public interface IGetPollsWithDetails
{
    Task<List<PollEntity>> GetPollsAsync(PollStatusEnum? status, ulong? userId, CancellationToken cancellationToken = default);
    Task<List<PollOptionsEntity>> GetPollOptionsByPollIdsAsync(List<int> pollIds, CancellationToken cancellationToken = default);
    Task<List<Vote>> GetVotesByPollIdsAsync(List<int> pollIds, CancellationToken cancellationToken = default);
}
