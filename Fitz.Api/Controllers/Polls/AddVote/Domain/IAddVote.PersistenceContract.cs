using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.AddVote.Domain;

public interface IAddVote
{
    Task<PollEntity?> FindPollByIdAsync(int pollId, CancellationToken cancellationToken = default);
    Task<PollOptionsEntity?> FindPollOptionAsync(int pollId, int optionId, CancellationToken cancellationToken = default);
    Task<AccountEntity?> FindAccountByIdAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<Vote?> FindVoteAsync(int pollId, ulong userId, CancellationToken cancellationToken = default);
    Task CreateVoteAsync(Vote vote, CancellationToken cancellationToken = default);
}
