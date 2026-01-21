namespace Fitz.Api.Controllers.Users.GetUsers.Domain;

public record GetUsersCommand(string? Query, int Page, int PageSize)
{
    public static GetUsersCommand From(string? query, int page, int pageSize)
    {
        return new GetUsersCommand(Query: query, Page: page, PageSize: pageSize);
    }
}
