using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.GetPollOptions.Domain;

public interface IGetPollOptions
{
    Task<PollEntity?> FindPollByIdAsync(int pollId, CancellationToken cancellationToken = default);
    Task<List<PollOptionsEntity>> GetPollOptionsAsync(int pollId, CancellationToken cancellationToken = default);
}
