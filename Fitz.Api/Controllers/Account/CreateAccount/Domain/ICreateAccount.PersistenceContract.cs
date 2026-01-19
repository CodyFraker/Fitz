using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Account.CreateAccount.Domain;
public interface ICreateAccount
{
    Task Save(CreateAccountModel model, CancellationToken cancellationToken = default);
    Task<AccountEntity?> FindByIdAsync(ulong id, CancellationToken cancellationToken = default);
}

