namespace Fitz.Api.Controllers.Admin.GetUsersWithFavorability.Domain;

public record GetUsersWithFavorabilityCommand(string? Query, int Skip, int Take, string? SortBy, string? SortOrder)
{
    public static GetUsersWithFavorabilityCommand From(string? query, int skip, int take, string? sortBy, string? sortOrder)
    {
        return new GetUsersWithFavorabilityCommand(
            Query: query,
            Skip: skip,
            Take: take,
            SortBy: sortBy,
            SortOrder: sortOrder ?? "asc"
        );
    }
}
