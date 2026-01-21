using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Admin.AdminDeletePoll.Domain;

public interface IAdminDeletePoll
{
    Task<PollEntity?> FindPollByIdAsync(int id, CancellationToken cancellationToken = default);
    Task DeletePollAsync(int pollId, CancellationToken cancellationToken = default);
}
