using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Admin.AdminBulkUpdateFavorability.Domain;

public interface IAdminBulkUpdateFavorability
{
    Task<AccountEntity?> FindAccountByIdAsync(ulong userId, CancellationToken cancellationToken = default);
}
