using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Admin.AdminUpdateFavorability.Domain;

public interface IAdminUpdateFavorability
{
    Task<AccountEntity?> FindAccountByIdAsync(ulong userId, CancellationToken cancellationToken = default);
}
