using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Users.GetUsers.Domain;

public interface IGetUsers
{
    Task<(List<AccountEntity> Accounts, int TotalCount)> GetUsersAsync(string? query, int skip, int take, CancellationToken cancellationToken = default);
}
