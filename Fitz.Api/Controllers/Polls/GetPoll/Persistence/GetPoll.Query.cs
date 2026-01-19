using Fitz.Api.Controllers.Polls.GetPoll.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Polls.GetPoll.Persistence;

public class GetPoll(IDbContextFactory<BotContext> contextFactory, ILogger<GetPoll> logger) : IGetPoll
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<GetPoll> _logger = logger;

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

    public async Task<PollEntity?> FindPollByMessageIdAsync(ulong messageId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Finding poll by message ID. MessageId: {MessageId}", messageId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var poll = await context.Polls
            .Where(p => p.MessageId == messageId)
            .FirstOrDefaultAsync(cancellationToken);

        if (poll != null)
        {
            _logger.LogInformation("Poll found. PollId: {PollId}, MessageId: {MessageId}", poll.Id, messageId);
        }
        else
        {
            _logger.LogInformation("Poll not found. MessageId: {MessageId}", messageId);
        }

        return poll;
    }
}
