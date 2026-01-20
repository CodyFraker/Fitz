using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.CreateRename.Domain;

public interface ICreateRename
{
    Task<AccountEntity?> FindAccountByIdAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<RenamesEntity> CreateRenameAsync(RenamesEntity rename, CancellationToken cancellationToken = default);
    Task<RenamesEntity?> FindRenameAfterCreationAsync(ulong affectedUserId, ulong requestedUserId, string newName, DateTime timestamp, CancellationToken cancellationToken = default);
}
