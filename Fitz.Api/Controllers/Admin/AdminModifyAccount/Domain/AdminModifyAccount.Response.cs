using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Admin.AdminModifyAccount.Domain;

public record AdminModifyAccountResponse(
    AccountEntity Account)
{
    public static AdminModifyAccountResponse From(AdminModifyAccountModel model)
    {
        return new AdminModifyAccountResponse(Account: model.Account);
    }
}
