using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.GetRename.Domain;

public interface IGetRename
{
    Task<RenamesEntity?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
}
