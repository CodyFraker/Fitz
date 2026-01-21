using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Admin.GetUsersWithFavorability.Domain;

public interface IGetUsersWithFavorability
{
    Task<(List<AccountEntity> Accounts, int TotalCount)> GetUsersAsync(string? query, int skip, int take, string? sortBy, string? sortOrder, CancellationToken cancellationToken = default);
    Task<AccountEntity?> FindAccountByIdAsync(ulong userId, CancellationToken cancellationToken = default);
}
