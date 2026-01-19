using Fitz.Api.Controllers.Polls.PostPollToPending.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Polls.PostPollToPending.Persistence;

public class PostPollToPending(IDbContextFactory<BotContext> contextFactory, ILogger<PostPollToPending> logger) : IPostPollToPending
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<PostPollToPending> _logger = logger;

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

    public async Task UpdatePollAsync(PollEntity poll, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating poll. PollId: {PollId}, MessageId: {MessageId}", poll.Id, poll.MessageId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.Polls.Update(poll);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Poll updated successfully. PollId: {PollId}, MessageId: {MessageId}", poll.Id, poll.MessageId);
    }
}
