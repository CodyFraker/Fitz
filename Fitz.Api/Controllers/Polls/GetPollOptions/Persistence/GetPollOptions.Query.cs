using Fitz.Api.Controllers.Polls.GetPollOptions.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Polls.GetPollOptions.Persistence;

public class GetPollOptions(IDbContextFactory<BotContext> contextFactory, ILogger<GetPollOptions> logger) : IGetPollOptions
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<GetPollOptions> _logger = logger;

    public async Task<PollEntity?> FindPollByIdAsync(int pollId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Finding poll by ID. PollId: {PollId}", pollId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var poll = await context.Polls
            .Where(p => p.Id == pollId)
            .FirstOrDefaultAsync(cancellationToken);

        if (poll != null)
        {
            _logger.LogInformation("Poll found. PollId: {PollId}", pollId);
        }
        else
        {
            _logger.LogInformation("Poll not found. PollId: {PollId}", pollId);
        }

        return poll;
    }

    public async Task<List<PollOptionsEntity>> GetPollOptionsAsync(int pollId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting poll options. PollId: {PollId}", pollId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var options = await context.PollsOptions
            .Where(o => o.PollId == pollId)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Poll options retrieved. PollId: {PollId}, Count: {Count}", pollId, options.Count);

        return options;
    }
}
