using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.UpdateRenameStatus.Domain;

public interface IUpdateRenameStatus
{
    Task<RenamesEntity?> FindRenameByIdAsync(int renameId, CancellationToken cancellationToken = default);
    Task<RenamesEntity> UpdateRenameAsync(RenamesEntity rename, CancellationToken cancellationToken = default);
}
