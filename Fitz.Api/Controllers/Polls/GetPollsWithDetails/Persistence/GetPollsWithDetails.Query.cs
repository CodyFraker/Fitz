using Fitz.Api.Controllers.Polls.GetPollsWithDetails.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Polls.GetPollsWithDetails.Persistence;

public class GetPollsWithDetails(IDbContextFactory<BotContext> contextFactory, ILogger<GetPollsWithDetails> logger) : IGetPollsWithDetails
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<GetPollsWithDetails> _logger = logger;

    public async Task<List<PollEntity>> GetPollsAsync(PollStatusEnum? status, ulong? userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting polls. Status: {Status}, UserId: {UserId}", status, userId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var query = context.Polls.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }
        else
        {
            query = query.Where(p => p.Status == PollStatusEnum.Approved);
        }

        if (userId.HasValue)
        {
            query = query.Where(p => p.AccountId == userId.Value);
        }

        var polls = await query.ToListAsync(cancellationToken);

        _logger.LogInformation("Polls retrieved. Count: {Count}", polls.Count);

        return polls;
    }

    public async Task<List<PollOptionsEntity>> GetPollOptionsByPollIdsAsync(List<int> pollIds, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting poll options by poll IDs. Count: {Count}", pollIds.Count);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var options = await context.PollsOptions
            .Where(o => pollIds.Contains(o.PollId))
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Poll options retrieved. Count: {Count}", options.Count);

        return options;
    }

    public async Task<List<Vote>> GetVotesByPollIdsAsync(List<int> pollIds, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting votes by poll IDs. Count: {Count}", pollIds.Count);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var votes = await context.Votes
            .Where(v => pollIds.Contains(v.PollId))
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Votes retrieved. Count: {Count}", votes.Count);

        return votes;
    }
}
