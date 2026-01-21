using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Users.GetUsers.Domain;

public record GetUsersModel(
    List<AccountEntity> Accounts,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages)
{
    public static GetUsersModel From(List<AccountEntity> accounts, int totalCount, int page, int pageSize, int totalPages)
    {
        return new GetUsersModel(
            Accounts: accounts,
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize,
            TotalPages: totalPages
        );
    }
}
