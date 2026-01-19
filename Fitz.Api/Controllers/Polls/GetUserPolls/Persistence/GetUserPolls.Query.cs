using Fitz.Api.Controllers.Polls.GetUserPolls.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Polls.GetUserPolls.Persistence;

public class GetUserPolls(IDbContextFactory<BotContext> contextFactory, ILogger<GetUserPolls> logger) : IGetUserPolls
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<GetUserPolls> _logger = logger;

    public async Task<List<PollEntity>> GetPollsByUserIdAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting polls by user ID. UserId: {UserId}", userId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var polls = await context.Polls
            .Where(p => p.AccountId == userId)
            .OrderByDescending(p => p.SubmittedOn)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Polls by user ID retrieved. UserId: {UserId}, Count: {Count}", userId, polls.Count);

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
