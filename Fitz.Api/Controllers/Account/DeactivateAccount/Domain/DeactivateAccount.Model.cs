using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Account.DeactivateAccount.Domain;

public record DeactivateAccountModel(
    AccountEntity Account,
    bool Deactivated)
{
    public static DeactivateAccountModel From(AccountEntity account, bool deactivated)
    {
        return new DeactivateAccountModel(
            Account: account,
            Deactivated: deactivated
        );
    }
}
