using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.GetPollsWithDetails.Domain;

public record GetPollsWithDetailsCommand(
    PollStatusEnum? Status,
    ulong? UserId,
    int Skip,
    int Take,
    string SortBy,
    string SortOrder)
{
    public static GetPollsWithDetailsCommand From(
        PollStatusEnum? status,
        ulong? userId,
        int skip,
        int take,
        string sortBy,
        string sortOrder)
    {
        return new GetPollsWithDetailsCommand(
            Status: status,
            UserId: userId,
            Skip: skip,
            Take: take,
            SortBy: sortBy,
            SortOrder: sortOrder
        );
    }
}
