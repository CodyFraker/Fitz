using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.GetRenamesByUser.Domain;

public interface IGetRenamesByUser
{
    Task<List<RenamesEntity>> GetRenamesByAccountIdAsync(ulong accountId, CancellationToken cancellationToken = default);
}
