using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Users.GetUsers.Domain;

public record GetUsersResponse(
    List<AccountEntity> Accounts,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages)
{
    public static GetUsersResponse From(GetUsersModel model)
    {
        return new GetUsersResponse(
            Accounts: model.Accounts,
            TotalCount: model.TotalCount,
            Page: model.Page,
            PageSize: model.PageSize,
            TotalPages: model.TotalPages
        );
    }
}
