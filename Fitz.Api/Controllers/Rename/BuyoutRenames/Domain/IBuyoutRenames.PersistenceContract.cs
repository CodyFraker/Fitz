using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.BuyoutRenames.Domain;

public interface IBuyoutRenames
{
    Task<List<RenamesEntity>> GetRenamesByAccountIdAsync(ulong accountId, CancellationToken cancellationToken = default);
    Task UpdateRenameAsync(RenamesEntity rename, CancellationToken cancellationToken = default);
}
