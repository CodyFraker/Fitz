using Fitz.Api.Controllers.Admin.AdminDeletePoll.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Admin.AdminDeletePoll.Persistence;

public class AdminDeletePoll(IDbContextFactory<BotContext> contextFactory, ILogger<AdminDeletePoll> logger) : IAdminDeletePoll
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<AdminDeletePoll> _logger = logger;

    public async Task<PollEntity?> FindPollByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Finding poll by ID. Id: {Id}", id);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var poll = await context.Polls
            .Where(p => p.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (poll != null)
        {
            _logger.LogInformation("Poll found. Id: {Id}, Question: {Question}", id, poll.Question);
        }
        else
        {
            _logger.LogInformation("Poll not found. Id: {Id}", id);
        }

        return poll;
    }

    public async Task DeletePollAsync(int pollId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting poll. PollId: {PollId}", pollId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var votes = await context.Votes.Where(v => v.PollId == pollId).ToListAsync(cancellationToken);
        var options = await context.PollsOptions.Where(o => o.PollId == pollId).ToListAsync(cancellationToken);
        var poll = await context.Polls.Where(p => p.Id == pollId).FirstOrDefaultAsync(cancellationToken);

        if (poll != null)
        {
            context.Votes.RemoveRange(votes);
            context.PollsOptions.RemoveRange(options);
            context.Polls.Remove(poll);
            await context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Poll deleted successfully. PollId: {PollId}, VotesDeleted: {VotesDeleted}, OptionsDeleted: {OptionsDeleted}", 
                pollId, votes.Count, options.Count);
        }
    }
}
