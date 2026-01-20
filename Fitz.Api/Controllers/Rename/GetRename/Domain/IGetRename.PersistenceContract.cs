using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.GetRename.Domain;

public interface IGetRename
{
    Task<RenamesEntity?> FindRenameByIdAsync(int renameId, CancellationToken cancellationToken = default);
}
