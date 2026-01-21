using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Admin.AdminModifyAccount.Domain;

public record AdminModifyAccountModel(
    AccountEntity Account)
{
    public static AdminModifyAccountModel From(AccountEntity account)
    {
        return new AdminModifyAccountModel(Account: account);
    }
}
