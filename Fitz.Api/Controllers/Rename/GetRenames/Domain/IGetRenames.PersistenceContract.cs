using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.GetRenames.Domain;

public interface IGetRenames
{
    Task<List<RenamesEntity>> GetAllRenamesAsync(RenameStatusEnum? status, CancellationToken cancellationToken = default);
}
