using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.GetPolls.Domain;

public interface IGetPolls
{
    Task<List<PollEntity>> GetAllPollsAsync(CancellationToken cancellationToken = default);
    Task<List<PollEntity>> GetPollsByUserIdAsync(ulong userId, CancellationToken cancellationToken = default);
}
