using Fitz.Api.Controllers.Polls.GetPollVotes.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Polls.GetPollVotes.Persistence;

public class GetPollVotes(IDbContextFactory<BotContext> contextFactory, ILogger<GetPollVotes> logger) : IGetPollVotes
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<GetPollVotes> _logger = logger;

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

    public async Task<List<Vote>> GetPollVotesAsync(int pollId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting poll votes. PollId: {PollId}", pollId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var votes = await context.Votes
            .Where(v => v.PollId == pollId)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Poll votes retrieved. PollId: {PollId}, Count: {Count}", pollId, votes.Count);

        return votes;
    }
}
