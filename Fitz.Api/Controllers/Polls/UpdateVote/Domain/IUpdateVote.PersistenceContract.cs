using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.UpdateVote.Domain;

public interface IUpdateVote
{
    Task<PollEntity?> FindPollByIdAsync(int pollId, CancellationToken cancellationToken = default);
    Task<Vote?> FindVoteAsync(int pollId, ulong userId, CancellationToken cancellationToken = default);
    Task<AccountEntity?> FindAccountByIdAsync(ulong userId, CancellationToken cancellationToken = default);
    Task UpdateVoteAsync(Vote vote, CancellationToken cancellationToken = default);
}
