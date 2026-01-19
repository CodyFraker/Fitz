using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.CreatePoll.Domain;

public interface ICreatePoll
{
    Task<AccountEntity?> FindAccountByIdAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<SettingsEntity?> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<int> GetPendingPollsCountAsync(ulong accountId, CancellationToken cancellationToken = default);
    Task<PollEntity> CreatePollAsync(PollEntity poll, CancellationToken cancellationToken = default);
    Task<List<PollOptionsEntity>> CreatePollOptionsAsync(int pollId, List<PollOptionsEntity> options, CancellationToken cancellationToken = default);
    Task<PollEntity?> FindPollByMessageIdAsync(ulong messageId, CancellationToken cancellationToken = default);
}
