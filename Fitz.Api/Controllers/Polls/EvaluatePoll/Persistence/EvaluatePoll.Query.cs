using Fitz.Api.Controllers.Polls.EvaluatePoll.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Polls.EvaluatePoll.Persistence;

public class EvaluatePoll(IDbContextFactory<BotContext> contextFactory, ILogger<EvaluatePoll> logger) : IEvaluatePoll
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<EvaluatePoll> _logger = logger;

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

    public async Task UpdatePollAsync(PollEntity poll, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating poll. PollId: {PollId}, Status: {Status}", poll.Id, poll.Status);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.Polls.Update(poll);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Poll updated successfully. PollId: {PollId}, Status: {Status}", poll.Id, poll.Status);
    }
}
