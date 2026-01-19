using Fitz.Api.Controllers.Polls.GetPolls.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Polls.GetPolls.Persistence;

public class GetPolls(IDbContextFactory<BotContext> contextFactory, ILogger<GetPolls> logger) : IGetPolls
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<GetPolls> _logger = logger;

    public async Task<List<PollEntity>> GetAllPollsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all polls");

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var polls = await context.Polls
            .ToListAsync(cancellationToken);

        _logger.LogInformation("All polls retrieved. Count: {Count}", polls.Count);

        return polls;
    }

    public async Task<List<PollEntity>> GetPollsByUserIdAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting polls by user ID. UserId: {UserId}", userId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var polls = await context.Polls
            .Where(p => p.AccountId == userId)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Polls by user ID retrieved. UserId: {UserId}, Count: {Count}", userId, polls.Count);

        return polls;
    }
}
