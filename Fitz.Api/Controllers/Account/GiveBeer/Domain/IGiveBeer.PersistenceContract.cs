using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Account.GiveBeer.Domain;

public interface IGiveBeer
{
    Task<AccountEntity?> FindAccountByIdAsync(ulong id, CancellationToken cancellationToken = default);
    Microsoft.Extensions.DependencyInjection.IServiceScopeFactory GetScopeFactory();
}
